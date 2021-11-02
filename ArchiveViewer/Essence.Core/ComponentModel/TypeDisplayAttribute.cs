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
      get => m_displayName.ResourcePropertyName;
      set => m_displayName.ResourcePropertyName = value;
    }

    public string Description
    {
      get => m_description.ResourcePropertyName;
      set => m_description.ResourcePropertyName = value;
    }

    public Type ResourceType
    {
      get => m_resourceType;
      set
      {
        if (!(m_resourceType != value))
          return;
        m_resourceType = value;
        m_displayName.ResourceType = value;
        m_description.ResourceType = value;
      }
    }

    public string GetDisplayName() => m_displayName.GetLocalizedValue();

    public string GetDescription() => m_description.GetLocalizedValue();
  }
}
