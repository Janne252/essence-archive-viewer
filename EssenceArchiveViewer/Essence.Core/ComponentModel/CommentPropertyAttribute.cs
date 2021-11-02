using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class)]
  public class CommentPropertyAttribute : Attribute
  {
    public static readonly CommentPropertyAttribute Default = new(null);

    public CommentPropertyAttribute(string propertyName) => PropertyName = propertyName;

    public string PropertyName { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is CommentPropertyAttribute propertyAttribute && PropertyName == propertyAttribute.PropertyName;
    }

    public override int GetHashCode() => PropertyName == null ? 0 : PropertyName.GetHashCode();
  }
}
