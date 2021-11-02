using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Essence.Core.IO.Archive
{
  public sealed class Folder : INode
  {
    internal Folder(Archive archive, IList<INode> children, string name)
    {
      Archive = archive;
      Children = new ReadOnlyCollection<INode>(children);
      Name = name;
    }

    public Archive Archive { get; }

    public INode Parent { get; internal set; }

    public IReadOnlyList<INode> Children { get; }

    public string Name { get; }

    public string FullName => (Parent != null ? Parent.FullName + Name : Name) + Path.DirectorySeparatorChar.ToString();

    public override string ToString() => Name;
  }
}
