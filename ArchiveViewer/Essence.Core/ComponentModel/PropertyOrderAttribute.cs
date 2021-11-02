using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class PropertyOrderAttribute : Attribute
  {
    public PropertyOrderAttribute(int order) => Order = order;

    public int Order { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is PropertyOrderAttribute propertyOrderAttribute && propertyOrderAttribute.Order == Order;
    }

    public override int GetHashCode() => Order.GetHashCode();
  }
}
