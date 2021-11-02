// Decompiled with JetBrains decompiler
// Type: Essence.Core.Pipeline.Project
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      this.Name = name;
      iniSection.TryGetValue(nameof (Parent), out this.m_parentName);
      string relativePath1;
      this.DataGenericDirectory = iniSection.TryGetValue("DataGeneric", out relativePath1) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath1) : (string) null;
      string relativePath2;
      this.DataIntermediateDirectory = iniSection.TryGetValue("DataIntermediate", out relativePath2) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath2) : (string) null;
      string relativePath3;
      this.DataPreviewDirectory = iniSection.TryGetValue("DataPreview", out relativePath3) ? PipelineConfig.GetFullPath(pipelineRoot, relativePath3) : (string) null;
    }

    internal void Resolve(
      IDictionary<string, Project> parents,
      IDictionary<string, IDictionary<string, ReadOnlyDictionary<string, string>>> projectConfigSections)
    {
      if (!string.IsNullOrEmpty(this.m_parentName))
      {
        Project project;
        if (!parents.TryGetValue(this.m_parentName, out project))
          throw new ApplicationException(string.Format("{0} parent project [{1}] not found for project [{2}].", (object) "pipeline.ini", (object) this.m_parentName, (object) this.Name));
        this.Parent = project;
      }
      IDictionary<string, ReadOnlyDictionary<string, string>> dictionary;
      if (projectConfigSections.TryGetValue(this.Name, out dictionary))
        this.ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dictionary);
      else
        this.ConfigSections = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>((IDictionary<string, ReadOnlyDictionary<string, string>>) new Dictionary<string, ReadOnlyDictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase));
    }

    public string GetConfigValue(string section, string key) => this.ConfigSections[section][key];

    public bool TryGetConfigValue(string section, string key, out string value)
    {
      ReadOnlyDictionary<string, string> readOnlyDictionary;
      if (this.ConfigSections.TryGetValue(section, out readOnlyDictionary))
        return readOnlyDictionary.TryGetValue(key, out value);
      value = (string) null;
      return false;
    }

    public override string ToString() => this.Name;
  }
}
