using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
  public class NamePropertyAttribute : Attribute
  {
    public static readonly NamePropertyAttribute Default = new NamePropertyAttribute((string) null);

    public NamePropertyAttribute(string propertyName) => PropertyName = propertyName;

    public string PropertyName { get; }

    public bool IsNameRequired { get; set; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is NamePropertyAttribute propertyAttribute && PropertyName == propertyAttribute.PropertyName && IsNameRequired == propertyAttribute.IsNameRequired;
    }

    public override int GetHashCode() => (PropertyName != null ? PropertyName.GetHashCode() : 0) ^ IsNameRequired.GetHashCode();
  }
}
