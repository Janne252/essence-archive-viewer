using System;
using System.ComponentModel;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public class EditorPropertyAttribute : Attribute
  {
    public EditorPropertyAttribute(string propertyName, object propertyValue)
    {
      PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      PropertyValue = propertyValue;
    }

    public EditorPropertyAttribute(string propertyName, Type propertyType, string propertyValue)
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

    public EditorPropertyAttribute(string propertyName, bool value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, char value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, sbyte value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, byte value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, short value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, ushort value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, int value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, uint value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, long value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, ulong value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, float value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, double value)
      : this(propertyName, (object) value)
    {
    }

    public EditorPropertyAttribute(string propertyName, string value)
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

    public override int GetHashCode() => PropertyName.GetHashCode() ^ (PropertyValue != null ? PropertyValue.GetHashCode() : 0);
  }
}
