using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class CategoryOrderAttribute : Attribute
  {
    public CategoryOrderAttribute(string category, int order)
    {
      Category = category ?? throw new ArgumentNullException(nameof (category));
      Order = order;
    }

    public string Category { get; }

    public int Order { get; }

    public override object TypeId => this;

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is CategoryOrderAttribute categoryOrderAttribute && Category == categoryOrderAttribute.Category && categoryOrderAttribute.Order == Order;
    }

    public override int GetHashCode() => Category.GetHashCode() ^ Order.GetHashCode();
  }
}
