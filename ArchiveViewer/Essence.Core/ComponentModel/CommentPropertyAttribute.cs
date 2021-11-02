// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.CommentPropertyAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class)]
  public class CommentPropertyAttribute : Attribute
  {
    public static readonly CommentPropertyAttribute Default = new CommentPropertyAttribute((string) null);

    public CommentPropertyAttribute(string propertyName) => this.PropertyName = propertyName;

    public string PropertyName { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is CommentPropertyAttribute propertyAttribute && this.PropertyName == propertyAttribute.PropertyName;
    }

    public override int GetHashCode() => this.PropertyName == null ? 0 : this.PropertyName.GetHashCode();
  }
}
