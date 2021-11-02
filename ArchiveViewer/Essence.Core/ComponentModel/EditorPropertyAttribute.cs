// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.EditorPropertyAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.ComponentModel;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public class EditorPropertyAttribute : Attribute
  {
    public EditorPropertyAttribute(string propertyName, object propertyValue)
    {
      this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      this.PropertyValue = propertyValue;
    }

    public EditorPropertyAttribute(string propertyName, Type propertyType, string propertyValue)
    {
      this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      try
      {
        this.PropertyValue = TypeDescriptor.GetConverter(propertyType).ConvertFromInvariantString(propertyValue);
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

    public override object TypeId => (object) (this.GetType().FullName + this.PropertyName);

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is EditorPropertyAttribute propertyAttribute && this.PropertyName == propertyAttribute.PropertyName && propertyAttribute.PropertyValue == this.PropertyValue;
    }

    public override int GetHashCode() => this.PropertyName.GetHashCode() ^ (this.PropertyValue != null ? this.PropertyValue.GetHashCode() : 0);
  }
}
