using Essence.Core.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Essence.Core.Pipeline
{
  public class PipelineConfig
  {
    public const string PipelineFileName = "pipeline.ini";

    public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> ConfigSections { get; }

    public ReadOnlyDictionary<string, Project> Projects { get; }

    public string PipelineRoot { get; }

    public string PipelinePath => Path.Combine(PipelineRoot, "pipeline.ini");

    public PipelineConfig(IEnumerable<string> searchStartPaths)
    {
      foreach (var searchStartPath in searchStartPaths)
      {
          if (LoadPipelineIni(searchStartPath, out var pipelineRoot, out var configSections, out var projects))
        {
          PipelineRoot = pipelineRoot;
          ConfigSections = configSections;
          Projects = projects;
          return;
        }
      }
      throw new ApplicationException(
          $"No {"pipeline.ini"} file found in any parents of the paths [{string.Join(";", searchStartPaths)}].");
    }

    public PipelineConfig()
      : this(new string[2]
      {
          Environment.CurrentDirectory,
          Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
      })
    {
    }

    public string GetConfigValue(string section, string key) => ConfigSections[section][key];

    public string GetConfigValue(string section, string project, string key)
    {
        return TryGetProjectConfigValue(section, project, key, out var str) ? str : GetConfigValue(section, key);
    }

    public bool TryGetConfigValue(string section, string key, out string value)
    {
        if (ConfigSections.TryGetValue(section, out var readOnlyDictionary))
        return readOnlyDictionary.TryGetValue(key, out value);
      value = null;
      return false;
    }

    public bool TryGetConfigValue(string section, string project, string key, out string value) => TryGetProjectConfigValue(section, project, key, out value) || TryGetConfigValue(section, key, out value);

    private bool TryGetProjectConfigValue(
      string section,
      string projectName,
      string key,
      out string value)
    {
        if (projectName != null && Projects.TryGetValue(projectName, out var project1))
      {
        foreach (var project2 in project1.DependencyChain)
        {
          if (project2.TryGetConfigValue(section, key, out value))
            return true;
        }
      }
      value = null;
      return false;
    }

    public string GetFullPath(string relativePath) => GetFullPath(PipelineRoot, relativePath);

    internal static string GetFullPath(string pipelineRoot, string relativePath) => PathUtil.EnsureTrailingDirectorySeparatorChar(PathUtil.CanonicalizePath(Path.Combine(pipelineRoot, relativePath)));

    private static bool LoadPipelineIni(
      string searchStartPath,
      out string pipelineRoot,
      out ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> configSections,
      out ReadOnlyDictionary<string, Project> projects)
    {
      for (var str = searchStartPath; str != null; str = Path.GetDirectoryName(str))
      {
        if (File.Exists(Path.Combine(str, "pipeline.ini")))
        {
          pipelineRoot = str;
          ParsePipelineIni(pipelineRoot, out configSections, out projects);
          return true;
        }
      }
      pipelineRoot = null;
      configSections = null;
      projects = null;
      return false;
    }

    private static void ParsePipelineIni(
      string pipelineRoot,
      out ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> readOnlyConfigSections,
      out ReadOnlyDictionary<string, Project> readOnlyProjects)
    {
      var iniFile = new IniFile();
      iniFile.Read(Path.Combine(pipelineRoot, "pipeline.ini"));
      var dictionary1 = new Dictionary<string, ReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
      var parents = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);
      var projectConfigSections = new Dictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);
      foreach (var section in iniFile.Sections)
      {
        var length = section.Key.IndexOf(':');
        if (length != -1)
        {
          var key = section.Key.Substring(0, length);
          var str = section.Key.Substring(length + 1);
          if (key.Equals("project", StringComparison.InvariantCultureIgnoreCase))
          {
            parents.Add(str, new Project(pipelineRoot, str, section.Value));
          }
          else
          {
              if (!projectConfigSections.TryGetValue(str, out var dictionary2))
            {
              dictionary2 = new Dictionary<string, ReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
              projectConfigSections.Add(str, dictionary2);
            }
            dictionary2.Add(key, new ReadOnlyDictionary<string, string>(section.Value));
          }
        }
        else
          dictionary1.Add(section.Key, new ReadOnlyDictionary<string, string>(section.Value));
      }
      foreach (var project in parents.Values)
        project.Resolve(parents, projectConfigSections);
      readOnlyConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dictionary1);
      readOnlyProjects = new ReadOnlyDictionary<string, Project>(parents);
    }
  }
}
