using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class PropertyEmphasisAttributeAttribute : Attribute
  {
    public static readonly PropertyEmphasisAttributeAttribute Default = new(PropertyEmphasis.Default);

    public PropertyEmphasisAttributeAttribute(PropertyEmphasis propertyEmphasis) => PropertyEmphasis = propertyEmphasis;

    public PropertyEmphasis PropertyEmphasis { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is PropertyEmphasisAttributeAttribute attributeAttribute && attributeAttribute.PropertyEmphasis == PropertyEmphasis;
    }

    public override int GetHashCode() => PropertyEmphasis.GetHashCode();
  }
}
