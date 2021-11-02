// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.DisplayIconAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
  public class DisplayIconAttribute : Attribute
  {
    public static readonly DisplayIconAttribute Default = new DisplayIconAttribute();

    public DisplayIconAttribute()
      : this(string.Empty)
    {
    }

    public DisplayIconAttribute(string displayIcon) => this.DisplayIcon = displayIcon;

    public string DisplayIcon { get; private set; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is DisplayIconAttribute displayIconAttribute && displayIconAttribute.DisplayIcon == this.DisplayIcon;
    }

    public override int GetHashCode() => this.DisplayIcon.GetHashCode();

    public override bool IsDefaultAttribute() => this.Equals((object) DisplayIconAttribute.Default);
  }
}
