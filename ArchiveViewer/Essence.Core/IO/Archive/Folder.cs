// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Archive.Folder
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Essence.Core.IO.Archive
{
  public sealed class Folder : INode
  {
    internal Folder(Essence.Core.IO.Archive.Archive archive, IList<INode> children, string name)
    {
      this.Archive = archive;
      this.Children = (IReadOnlyList<INode>) new ReadOnlyCollection<INode>(children);
      this.Name = name;
    }

    public Essence.Core.IO.Archive.Archive Archive { get; }

    public INode Parent { get; internal set; }

    public IReadOnlyList<INode> Children { get; }

    public string Name { get; }

    public string FullName => (this.Parent != null ? this.Parent.FullName + this.Name : this.Name) + Path.DirectorySeparatorChar.ToString();

    public override string ToString() => this.Name;
  }
}
