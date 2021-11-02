// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.ListElementEditorAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
  public sealed class ListElementEditorAttribute : Attribute
  {
    private string m_typeId;

    public ListElementEditorAttribute()
      : this(string.Empty, string.Empty)
    {
    }

    public ListElementEditorAttribute(string editorTypeName, string editorBaseTypeName)
    {
      this.EditorTypeName = editorTypeName;
      this.EditorBaseTypeName = editorBaseTypeName;
    }

    public ListElementEditorAttribute(string editorTypeName, Type baseType)
      : this(editorTypeName, baseType.AssemblyQualifiedName)
    {
    }

    public ListElementEditorAttribute(Type type, Type baseType)
      : this(type.AssemblyQualifiedName, baseType.AssemblyQualifiedName)
    {
    }

    public string EditorBaseTypeName { get; }

    public string EditorTypeName { get; }

    public override object TypeId
    {
      get
      {
        if (this.m_typeId == null)
        {
          string str = this.EditorBaseTypeName;
          int length = str.IndexOf(',');
          if (length != -1)
            str = str.Substring(0, length);
          this.m_typeId = this.GetType().FullName + str;
        }
        return (object) this.m_typeId;
      }
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is ListElementEditorAttribute elementEditorAttribute && elementEditorAttribute.EditorTypeName == this.EditorTypeName && elementEditorAttribute.EditorBaseTypeName == this.EditorBaseTypeName;
    }

    public override int GetHashCode()
    {
      string editorTypeName = this.EditorTypeName;
      int num1 = editorTypeName != null ? editorTypeName.GetHashCode() : 0;
      string editorBaseTypeName = this.EditorBaseTypeName;
      int num2 = editorBaseTypeName != null ? editorBaseTypeName.GetHashCode() : 0;
      return num1 ^ num2;
    }
  }
}
