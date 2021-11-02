// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.TypeDisplayAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false)]
  public sealed class TypeDisplayAttribute : Attribute
  {
    private Type m_resourceType;
    private LocalizableString m_displayName;
    private LocalizableString m_description;

    public string DisplayName
    {
      get => this.m_displayName.ResourcePropertyName;
      set => this.m_displayName.ResourcePropertyName = value;
    }

    public string Description
    {
      get => this.m_description.ResourcePropertyName;
      set => this.m_description.ResourcePropertyName = value;
    }

    public Type ResourceType
    {
      get => this.m_resourceType;
      set
      {
        if (!(this.m_resourceType != value))
          return;
        this.m_resourceType = value;
        this.m_displayName.ResourceType = value;
        this.m_description.ResourceType = value;
      }
    }

    public string GetDisplayName() => this.m_displayName.GetLocalizedValue();

    public string GetDescription() => this.m_description.GetLocalizedValue();
  }
}
