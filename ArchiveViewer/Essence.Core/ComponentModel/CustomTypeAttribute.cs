using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class CustomTypeAttribute : Attribute
  {
    public CustomTypeAttribute(ICustomType customType) => CustomType = customType ?? throw new ArgumentNullException(nameof (customType));

    public ICustomType CustomType { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is CustomTypeAttribute customTypeAttribute && CustomType == customTypeAttribute.CustomType;
    }

    public override int GetHashCode() => CustomType.GetHashCode();
  }
}
