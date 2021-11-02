// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.LocalizableString
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      get => this.m_resourceType;
      set
      {
        if (!(this.m_resourceType != value))
          return;
        this.ClearLocalizedValue();
        this.m_resourceType = value;
      }
    }

    public string ResourcePropertyName
    {
      get => this.m_resourcePropertyName;
      set
      {
        if (!(this.m_resourcePropertyName != value))
          return;
        this.ClearLocalizedValue();
        this.m_resourcePropertyName = value;
      }
    }

    public string GetLocalizedValue()
    {
      if (this.m_getLocalizedValue == null)
      {
        if (this.m_resourceType == (Type) null || this.m_resourcePropertyName == null)
        {
          string resourcePropertyName = this.m_resourcePropertyName;
          this.m_getLocalizedValue = (Func<string>) (() => resourcePropertyName);
        }
        else
        {
          Type resourceType = this.m_resourceType;
          if (this.m_resourceType.IsVisible)
          {
            string resourcePropertyName = this.m_resourcePropertyName;
            PropertyInfo property = resourceType.GetProperty(resourcePropertyName);
            if (property != (PropertyInfo) null)
            {
              if (property.PropertyType == typeof (string))
              {
                MethodInfo getMethod = property.GetGetMethod();
                this.m_getLocalizedValue = !(getMethod != (MethodInfo) null) ? (Func<string>) (() =>
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
                this.m_getLocalizedValue = (Func<string>) (() =>
                {
                  throw new InvalidOperationException(string.Format("Property [{0}.{1}] is not of type [{2}].", (object) resourceType, (object) resourcePropertyName, (object) typeof (string)));
                });
            }
            else
              this.m_getLocalizedValue = (Func<string>) (() =>
              {
                throw new InvalidOperationException(string.Format("Property [{0}] not found on [{1}].", (object) resourcePropertyName, (object) resourceType));
              });
          }
          else
            this.m_getLocalizedValue = (Func<string>) (() =>
            {
              throw new InvalidOperationException(string.Format("Type [{0}] is not visible.", (object) resourceType));
            });
        }
      }
      return this.m_getLocalizedValue();
    }

    private void ClearLocalizedValue() => this.m_getLocalizedValue = (Func<string>) null;
  }
}
