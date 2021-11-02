// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.FilterAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Property)]
  public class FilterAttribute : Attribute
  {
    public FilterAttribute(FilterOperation operation, string filter)
    {
      this.Operation = operation;
      this.Filter = filter ?? throw new ArgumentNullException(nameof (filter));
    }

    public FilterOperation Operation { get; }

    public string Filter { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is FilterAttribute filterAttribute && this.Operation == filterAttribute.Operation && this.Filter == filterAttribute.Filter;
    }

    public override int GetHashCode() => this.Operation.GetHashCode() ^ this.Filter.GetHashCode();
  }
}
