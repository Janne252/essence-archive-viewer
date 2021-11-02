// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.NamePropertyAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
  public class NamePropertyAttribute : Attribute
  {
    public static readonly NamePropertyAttribute Default = new NamePropertyAttribute((string) null);

    public NamePropertyAttribute(string propertyName) => this.PropertyName = propertyName;

    public string PropertyName { get; }

    public bool IsNameRequired { get; set; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is NamePropertyAttribute propertyAttribute && this.PropertyName == propertyAttribute.PropertyName && this.IsNameRequired == propertyAttribute.IsNameRequired;
    }

    public override int GetHashCode() => (this.PropertyName != null ? this.PropertyName.GetHashCode() : 0) ^ this.IsNameRequired.GetHashCode();
  }
}
