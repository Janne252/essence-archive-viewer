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
      foreach (string searchStartPath in searchStartPaths)
      {
        string pipelineRoot;
        ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> configSections;
        ReadOnlyDictionary<string, Project> projects;
        if (LoadPipelineIni(searchStartPath, out pipelineRoot, out configSections, out projects))
        {
          PipelineRoot = pipelineRoot;
          ConfigSections = configSections;
          Projects = projects;
          return;
        }
      }
      throw new ApplicationException(string.Format("No {0} file found in any parents of the paths [{1}].", (object) "pipeline.ini", (object) string.Join(";", searchStartPaths)));
    }

    public PipelineConfig()
      : this((IEnumerable<string>) new string[2]
      {
        Environment.CurrentDirectory,
        Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
      })
    {
    }

    public string GetConfigValue(string section, string key) => ConfigSections[section][key];

    public string GetConfigValue(string section, string project, string key)
    {
      string str;
      return TryGetProjectConfigValue(section, project, key, out str) ? str : GetConfigValue(section, key);
    }

    public bool TryGetConfigValue(string section, string key, out string value)
    {
      ReadOnlyDictionary<string, string> readOnlyDictionary;
      if (ConfigSections.TryGetValue(section, out readOnlyDictionary))
        return readOnlyDictionary.TryGetValue(key, out value);
      value = (string) null;
      return false;
    }

    public bool TryGetConfigValue(string section, string project, string key, out string value) => TryGetProjectConfigValue(section, project, key, out value) || TryGetConfigValue(section, key, out value);

    private bool TryGetProjectConfigValue(
      string section,
      string projectName,
      string key,
      out string value)
    {
      Project project1;
      if (projectName != null && Projects.TryGetValue(projectName, out project1))
      {
        foreach (Project project2 in project1.DependencyChain)
        {
          if (project2.TryGetConfigValue(section, key, out value))
            return true;
        }
      }
      value = (string) null;
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
      for (string str = searchStartPath; str != null; str = Path.GetDirectoryName(str))
      {
        if (File.Exists(Path.Combine(str, "pipeline.ini")))
        {
          pipelineRoot = str;
          ParsePipelineIni(pipelineRoot, out configSections, out projects);
          return true;
        }
      }
      pipelineRoot = (string) null;
      configSections = (ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>) null;
      projects = (ReadOnlyDictionary<string, Project>) null;
      return false;
    }

    private static void ParsePipelineIni(
      string pipelineRoot,
      out ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> readOnlyConfigSections,
      out ReadOnlyDictionary<string, Project> readOnlyProjects)
    {
      IniFile iniFile = new IniFile();
      iniFile.Read(Path.Combine(pipelineRoot, "pipeline.ini"));
      Dictionary<string, ReadOnlyDictionary<string, string>> dictionary1 = new Dictionary<string, ReadOnlyDictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      Dictionary<string, Project> parents = new Dictionary<string, Project>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      Dictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>> projectConfigSections = new Dictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      foreach (KeyValuePair<string, Dictionary<string, string>> section in iniFile.Sections)
      {
        int length = section.Key.IndexOf(':');
        if (length != -1)
        {
          string key = section.Key.Substring(0, length);
          string str = section.Key.Substring(length + 1);
          if (key.Equals("project", StringComparison.InvariantCultureIgnoreCase))
          {
            parents.Add(str, new Project(pipelineRoot, str, (IDictionary<string, string>) section.Value));
          }
          else
          {
            IDictionary<string, ReadOnlyDictionary<string, string>> dictionary2;
            if (!projectConfigSections.TryGetValue(str, out dictionary2))
            {
              dictionary2 = (IDictionary<string, ReadOnlyDictionary<string, string>>) new Dictionary<string, ReadOnlyDictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
              projectConfigSections.Add(str, dictionary2);
            }
            dictionary2.Add(key, new ReadOnlyDictionary<string, string>((IDictionary<string, string>) section.Value));
          }
        }
        else
          dictionary1.Add(section.Key, new ReadOnlyDictionary<string, string>((IDictionary<string, string>) section.Value));
      }
      foreach (Project project in parents.Values)
        project.Resolve((IDictionary<string, Project>) parents, (IDictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>>) projectConfigSections);
      readOnlyConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>((IDictionary<string, ReadOnlyDictionary<string, string>>) dictionary1);
      readOnlyProjects = new ReadOnlyDictionary<string, Project>((IDictionary<string, Project>) parents);
    }
  }
}
