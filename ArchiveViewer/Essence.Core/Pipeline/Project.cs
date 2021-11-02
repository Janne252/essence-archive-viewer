using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Essence.Core.Pipeline
{
  public class Project
  {
    private string m_parentName;

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
        Project project = this;
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
      string relativePath1;
      DataGenericDirectory = iniSection.TryGetValue("DataGeneric", out relativePath1) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath1) : (string) null;
      string relativePath2;
      DataIntermediateDirectory = iniSection.TryGetValue("DataIntermediate", out relativePath2) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath2) : (string) null;
      string relativePath3;
      DataPreviewDirectory = iniSection.TryGetValue("DataPreview", out relativePath3) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath3) : (string) null;
    }

    internal void Resolve(
      IDictionary<string, Project> parents,
      IDictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>> projectConfigSections)
    {
      if (!string.IsNullOrEmpty(m_parentName))
      {
        Project project;
        if (!parents.TryGetValue(m_parentName, out project))
          throw new ApplicationException(string.Format("{0} parent project [{1}] not found for project [{2}].", (object) "pipeline.ini", (object) m_parentName, (object) Name));
        Parent = project;
      }
      IDictionary<string, ReadOnlyDictionary<string, string>> dictionary;
      if (projectConfigSections.TryGetValue(Name, out dictionary))
        ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dictionary);
      else
        ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>((IDictionary<string, ReadOnlyDictionary<string, string>>) new Dictionary<string, ReadOnlyDictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase));
    }

    public string GetConfigValue(string section, string key) => ConfigSections[section][key];

    public bool TryGetConfigValue(string section, string key, out string value)
    {
      ReadOnlyDictionary<string, string> readOnlyDictionary;
      if (ConfigSections.TryGetValue(section, out readOnlyDictionary))
        return readOnlyDictionary.TryGetValue(key, out value);
      value = (string) null;
      return false;
    }

    public override string ToString() => Name;
  }
}
