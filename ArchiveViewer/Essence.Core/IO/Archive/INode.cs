using System.Collections.Generic;

namespace Essence.Core.IO.Archive
{
  public interface INode
  {
    Archive Archive { get; }

    INode Parent { get; }

    IReadOnlyList<INode> Children { get; }

    string Name { get; }

    string FullName { get; }
  }
}
