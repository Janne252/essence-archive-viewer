using System;
using System.ComponentModel;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public sealed class ListElementEditorPropertyAttribute : Attribute
  {
    public ListElementEditorPropertyAttribute(string propertyName, object propertyValue)
    {
      PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      PropertyValue = propertyValue;
    }

    public ListElementEditorPropertyAttribute(
      string propertyName,
      Type propertyType,
      string propertyValue)
    {
      PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      try
      {
        PropertyValue = TypeDescriptor.GetConverter(propertyType).ConvertFromInvariantString(propertyValue);
      }
      catch
      {
      }
    }

    public ListElementEditorPropertyAttribute(string propertyName, bool value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, char value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, sbyte value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, byte value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, short value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, ushort value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, int value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, uint value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, long value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, ulong value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, float value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, double value)
      : this(propertyName, (object) value)
    {
    }

    public ListElementEditorPropertyAttribute(string propertyName, string value)
      : this(propertyName, (object) value)
    {
    }

    public string PropertyName { get; }

    public object PropertyValue { get; }

    public override object TypeId => (object) (GetType().FullName + PropertyName);

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is EditorPropertyAttribute propertyAttribute && PropertyName == propertyAttribute.PropertyName && propertyAttribute.PropertyValue == PropertyValue;
    }

    public override int GetHashCode()
    {
      int hashCode = PropertyName.GetHashCode();
      object propertyValue = PropertyValue;
      int num = propertyValue != null ? propertyValue.GetHashCode() : 0;
      return hashCode ^ num;
    }
  }
}
