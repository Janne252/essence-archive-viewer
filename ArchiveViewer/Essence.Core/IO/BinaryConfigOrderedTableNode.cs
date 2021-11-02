using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigOrderedTableNode : BinaryConfigTableNodeBase
  {
    public const uint NodeType = 101;

    public BinaryConfigOrderedTableNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigOrderedTableNode(DictionaryKey key)
      : base(key)
    {
    }

    internal static BinaryConfigOrderedTableNode Read(
      DictionaryKey key,
      BinaryReader binaryReader,
      KeyResolver keyResolver)
    {
      var orderedTableNode = new BinaryConfigOrderedTableNode(key);
      ReadChildren(binaryReader, keyResolver, orderedTableNode.Children);
      return orderedTableNode;
    }

    protected internal override uint GetNodeType() => 101;

    protected override IEnumerable<BinaryConfigNode> GetOrderedChildren() => Children;
  }
}
