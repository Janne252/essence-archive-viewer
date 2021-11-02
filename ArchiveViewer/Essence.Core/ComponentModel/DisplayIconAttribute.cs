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

    public DisplayIconAttribute(string displayIcon) => DisplayIcon = displayIcon;

    public string DisplayIcon { get; private set; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is DisplayIconAttribute displayIconAttribute && displayIconAttribute.DisplayIcon == DisplayIcon;
    }

    public override int GetHashCode() => DisplayIcon.GetHashCode();

    public override bool IsDefaultAttribute() => Equals((object) Default);
  }
}
