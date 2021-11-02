// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.CategoryOrderAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class CategoryOrderAttribute : Attribute
  {
    public CategoryOrderAttribute(string category, int order)
    {
      this.Category = category ?? throw new ArgumentNullException(nameof (category));
      this.Order = order;
    }

    public string Category { get; }

    public int Order { get; }

    public override object TypeId => (object) this;

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is CategoryOrderAttribute categoryOrderAttribute && this.Category == categoryOrderAttribute.Category && categoryOrderAttribute.Order == this.Order;
    }

    public override int GetHashCode() => this.Category.GetHashCode() ^ this.Order.GetHashCode();
  }
}
