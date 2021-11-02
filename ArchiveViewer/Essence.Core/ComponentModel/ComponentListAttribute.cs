// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.ComponentListAttribute
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core.ComponentModel
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
  public class ComponentListAttribute : Attribute
  {
    public static readonly ComponentListAttribute Yes = new ComponentListAttribute(true);
    public static readonly ComponentListAttribute No = new ComponentListAttribute(false);

    public ComponentListAttribute(bool componentList) => this.ComponentList = componentList;

    public bool ComponentList { get; }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      return obj is ComponentListAttribute componentListAttribute && this.ComponentList == componentListAttribute.ComponentList;
    }

    public override int GetHashCode() => this.ComponentList.GetHashCode();
  }
}
