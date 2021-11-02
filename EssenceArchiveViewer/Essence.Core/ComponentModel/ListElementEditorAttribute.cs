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
      EditorTypeName = editorTypeName;
      EditorBaseTypeName = editorBaseTypeName;
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
        if (m_typeId == null)
        {
          var str = EditorBaseTypeName;
          var length = str.IndexOf(',');
          if (length != -1)
            str = str.Substring(0, length);
          m_typeId = GetType().FullName + str;
        }
        return m_typeId;
      }
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is ListElementEditorAttribute elementEditorAttribute && elementEditorAttribute.EditorTypeName == EditorTypeName && elementEditorAttribute.EditorBaseTypeName == EditorBaseTypeName;
    }

    public override int GetHashCode()
    {
      var editorTypeName = EditorTypeName;
      var num1 = editorTypeName != null ? editorTypeName.GetHashCode() : 0;
      var editorBaseTypeName = EditorBaseTypeName;
      var num2 = editorBaseTypeName != null ? editorBaseTypeName.GetHashCode() : 0;
      return num1 ^ num2;
    }
  }
}
