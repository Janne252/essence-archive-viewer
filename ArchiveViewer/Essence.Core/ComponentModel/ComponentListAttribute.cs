using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
  public class ComponentListAttribute : Attribute
  {
    public static readonly ComponentListAttribute Yes = new(true);
    public static readonly ComponentListAttribute No = new(false);

    public ComponentListAttribute(bool componentList) => ComponentList = componentList;

    public bool ComponentList { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is ComponentListAttribute componentListAttribute && ComponentList == componentListAttribute.ComponentList;
    }

    public override int GetHashCode() => ComponentList.GetHashCode();
  }
}
