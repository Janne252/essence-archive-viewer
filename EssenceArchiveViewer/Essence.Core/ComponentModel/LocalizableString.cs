using System;

namespace Essence.Core.ComponentModel
{
  public struct LocalizableString
  {
    private Type m_resourceType;
    private string m_resourcePropertyName;
    private Func<string> m_getLocalizedValue;

    public Type ResourceType
    {
      get => m_resourceType;
      set
      {
        if (!(m_resourceType != value))
          return;
        ClearLocalizedValue();
        m_resourceType = value;
      }
    }

    public string ResourcePropertyName
    {
      get => m_resourcePropertyName;
      set
      {
        if (!(m_resourcePropertyName != value))
          return;
        ClearLocalizedValue();
        m_resourcePropertyName = value;
      }
    }

    public string GetLocalizedValue()
    {
      if (m_getLocalizedValue == null)
      {
        if (m_resourceType == null || m_resourcePropertyName == null)
        {
          var resourcePropertyName = m_resourcePropertyName;
          m_getLocalizedValue = () => resourcePropertyName;
        }
        else
        {
          var resourceType = m_resourceType;
          if (m_resourceType.IsVisible)
          {
            var resourcePropertyName = m_resourcePropertyName;
            var property = resourceType.GetProperty(resourcePropertyName);
            if (property != null)
            {
              if (property.PropertyType == typeof (string))
              {
                var getMethod = property.GetGetMethod();
                m_getLocalizedValue = !(getMethod != null) ? () =>
                {
                    throw new InvalidOperationException(
                        $"Property [{resourceType}.{resourcePropertyName}] is not readable.");
                } : (!getMethod.IsStatic ? () =>
                {
                    throw new InvalidOperationException(
                        $"Property [{resourceType}.{resourcePropertyName}] is not static.");
                } : (!getMethod.IsPublic ? () =>
                {
                    throw new InvalidOperationException(
                        $"Property [{resourceType}.{resourcePropertyName}] is not public.");
                } : () => (string) property.GetValue(null, null)));
              }
              else
                m_getLocalizedValue = () =>
                {
                    throw new InvalidOperationException(
                        $"Property [{resourceType}.{resourcePropertyName}] is not of type [{typeof(string)}].");
                };
            }
            else
              m_getLocalizedValue = () =>
              {
                  throw new InvalidOperationException(
                      $"Property [{resourcePropertyName}] not found on [{resourceType}].");
              };
          }
          else
            m_getLocalizedValue = () =>
            {
                throw new InvalidOperationException($"Type [{resourceType}] is not visible.");
            };
        }
      }
      return m_getLocalizedValue();
    }

    private void ClearLocalizedValue() => m_getLocalizedValue = null;
  }
}
