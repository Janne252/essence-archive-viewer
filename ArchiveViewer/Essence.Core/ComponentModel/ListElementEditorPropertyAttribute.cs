// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.ListElementEditorPropertyAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.ComponentModel;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public sealed class ListElementEditorPropertyAttribute : Attribute
  {
    public ListElementEditorPropertyAttribute(string propertyName, object propertyValue)
    {
      this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof (propertyName));
      this.PropertyValue = propertyValue;
    }

    public ListElementEditorPropertyAttribute(
      string propertyName,
      Type propertyType,
      string propertyValue)
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

    public override object TypeId => (object) (this.GetType().FullName + this.PropertyName);

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is EditorPropertyAttribute propertyAttribute && this.PropertyName == propertyAttribute.PropertyName && propertyAttribute.PropertyValue == this.PropertyValue;
    }

    public override int GetHashCode()
    {
      int hashCode = this.PropertyName.GetHashCode();
      object propertyValue = this.PropertyValue;
      int num = propertyValue != null ? propertyValue.GetHashCode() : 0;
      return hashCode ^ num;
    }
  }
}
