using System;
using System.Reflection;

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
        if (m_resourceType == (Type) null || m_resourcePropertyName == null)
        {
          string resourcePropertyName = m_resourcePropertyName;
          m_getLocalizedValue = (Func<string>) (() => resourcePropertyName);
        }
        else
        {
          Type resourceType = m_resourceType;
          if (m_resourceType.IsVisible)
          {
            string resourcePropertyName = m_resourcePropertyName;
            PropertyInfo property = resourceType.GetProperty(resourcePropertyName);
            if (property != (PropertyInfo) null)
            {
              if (property.PropertyType == typeof (string))
              {
                MethodInfo getMethod = property.GetGetMethod();
                m_getLocalizedValue = !(getMethod != (MethodInfo) null) ? (Func<string>) (() =>
                {
                  throw new InvalidOperationException(string.Format("Property [{0}.{1}] is not readable.", (object) resourceType, (object) resourcePropertyName));
                }) : (!getMethod.IsStatic ? (Func<string>) (() =>
                {
                  throw new InvalidOperationException(string.Format("Property [{0}.{1}] is not static.", (object) resourceType, (object) resourcePropertyName));
                }) : (!getMethod.IsPublic ? (Func<string>) (() =>
                {
                  throw new InvalidOperationException(string.Format("Property [{0}.{1}] is not public.", (object) resourceType, (object) resourcePropertyName));
                }) : (Func<string>) (() => (string) property.GetValue((object) null, (object[]) null))));
              }
              else
                m_getLocalizedValue = (Func<string>) (() =>
                {
                  throw new InvalidOperationException(string.Format("Property [{0}.{1}] is not of type [{2}].", (object) resourceType, (object) resourcePropertyName, (object) typeof (string)));
                });
            }
            else
              m_getLocalizedValue = (Func<string>) (() =>
              {
                throw new InvalidOperationException(string.Format("Property [{0}] not found on [{1}].", (object) resourcePropertyName, (object) resourceType));
              });
          }
          else
            m_getLocalizedValue = (Func<string>) (() =>
            {
              throw new InvalidOperationException(string.Format("Type [{0}] is not visible.", (object) resourceType));
            });
        }
      }
      return m_getLocalizedValue();
    }

    private void ClearLocalizedValue() => m_getLocalizedValue = (Func<string>) null;
  }
}
