using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property)]
  public class FilterAttribute : Attribute
  {
    public FilterAttribute(FilterOperation operation, string filter)
    {
      Operation = operation;
      Filter = filter ?? throw new ArgumentNullException(nameof (filter));
    }

    public FilterOperation Operation { get; }

    public string Filter { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is FilterAttribute filterAttribute && Operation == filterAttribute.Operation && Filter == filterAttribute.Filter;
    }

    public override int GetHashCode() => Operation.GetHashCode() ^ Filter.GetHashCode();
  }
}
