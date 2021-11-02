using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Essence.Core.Pipeline
{
  public class Project
  {
    private readonly string m_parentName;

    public string Name { get; }

    public Project Parent { get; private set; }

    public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> ConfigSections { get; private set; }

    public string DataGenericDirectory { get; }

    public string DataIntermediateDirectory { get; }

    public string DataPreviewDirectory { get; }

    public IEnumerable<Project> DependencyChain
    {
      get
      {
        var project = this;
        do
        {
          yield return project;
          project = project.Parent;
        }
        while (project != null);
      }
    }

    internal Project(string pipelineRoot, string name, IDictionary<string, string> iniSection)
    {
      Name = name;
      iniSection.TryGetValue(nameof (Parent), out m_parentName);
      DataGenericDirectory = iniSection.TryGetValue("DataGeneric", out var relativePath1) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath1) : null;
      DataIntermediateDirectory = iniSection.TryGetValue("DataIntermediate", out var relativePath2) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath2) : null;
      DataPreviewDirectory = iniSection.TryGetValue("DataPreview", out var relativePath3) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath3) : null;
    }

    internal void Resolve(
      IDictionary<string, Project> parents,
      IDictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>> projectConfigSections)
    {
      if (!string.IsNullOrEmpty(m_parentName))
      {
          if (!parents.TryGetValue(m_parentName, out var project))
          throw new ApplicationException(
              $"{"pipeline.ini"} parent project [{m_parentName}] not found for project [{Name}].");
        Parent = project;
      }

      if (projectConfigSections.TryGetValue(Name, out var dictionary))
        ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dictionary);
      else
        ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(new Dictionary<string, ReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase));
    }

    public string GetConfigValue(string section, string key) => ConfigSections[section][key];

    public bool TryGetConfigValue(string section, string key, out string value)
    {
        if (ConfigSections.TryGetValue(section, out var readOnlyDictionary))
        return readOnlyDictionary.TryGetValue(key, out value);
      value = null;
      return false;
    }

    public override string ToString() => Name;
  }
}
