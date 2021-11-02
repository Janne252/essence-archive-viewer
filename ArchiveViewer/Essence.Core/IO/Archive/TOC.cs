// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Archive.TOC
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Essence.Core.IO.Archive
{
  public sealed class TOC : INode
  {
    internal TOC(Essence.Core.IO.Archive.Archive archive, IList<INode> children, string name, string alternateName)
    {
      this.Archive = archive;
      this.Children = (IReadOnlyList<INode>) new ReadOnlyCollection<INode>(children);
      this.Name = name;
      this.FullName = name.ToUpperInvariant() + ":" + Path.DirectorySeparatorChar.ToString();
      this.AlternateName = alternateName;
    }

    public Essence.Core.IO.Archive.Archive Archive { get; }

    public INode Parent => (INode) this.Archive;

    public IReadOnlyList<INode> Children { get; }

    public string Name { get; }

    public string FullName { get; }

    public string AlternateName { get; }

    public override string ToString() => this.Name;
  }
}
