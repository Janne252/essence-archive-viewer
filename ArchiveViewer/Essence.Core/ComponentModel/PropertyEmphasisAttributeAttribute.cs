// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.PropertyEmphasisAttributeAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class PropertyEmphasisAttributeAttribute : Attribute
  {
    public static readonly PropertyEmphasisAttributeAttribute Default = new PropertyEmphasisAttributeAttribute(PropertyEmphasis.Default);

    public PropertyEmphasisAttributeAttribute(PropertyEmphasis propertyEmphasis) => this.PropertyEmphasis = propertyEmphasis;

    public PropertyEmphasis PropertyEmphasis { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is PropertyEmphasisAttributeAttribute attributeAttribute && attributeAttribute.PropertyEmphasis == this.PropertyEmphasis;
    }

    public override int GetHashCode() => this.PropertyEmphasis.GetHashCode();
  }
}
