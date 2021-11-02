using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Essence.Core.IO.Archive
{
  public sealed class TOC : INode
  {
    internal TOC(Archive archive, IList<INode> children, string name, string alternateName)
    {
      Archive = archive;
      Children = (IReadOnlyList<INode>) new ReadOnlyCollection<INode>(children);
      Name = name;
      FullName = name.ToUpperInvariant() + ":" + Path.DirectorySeparatorChar.ToString();
      AlternateName = alternateName;
    }

    public Archive Archive { get; }

    public INode Parent => (INode) Archive;

    public IReadOnlyList<INode> Children { get; }

    public string Name { get; }

    public string FullName { get; }

    public string AlternateName { get; }

    public override string ToString() => Name;
  }
}
