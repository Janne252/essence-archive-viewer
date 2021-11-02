using System;
using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigTableNode : BinaryConfigTableNodeBase
  {
    public const uint NodeType = 100;

    public BinaryConfigTableNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigTableNode(DictionaryKey key)
      : base(key)
    {
    }

    internal static BinaryConfigTableNode Read(
      DictionaryKey key,
      BinaryReader binaryReader,
      KeyResolver keyResolver)
    {
      var binaryConfigTableNode = new BinaryConfigTableNode(key);
      ReadChildren(binaryReader, keyResolver, binaryConfigTableNode.Children);
      return binaryConfigTableNode;
    }

    protected internal override uint GetNodeType() => 100;

    protected override IEnumerable<BinaryConfigNode> GetOrderedChildren()
    {
      var array = Children.ToArray();
      Array.Sort<BinaryConfigNode>(array, (lhs, rhs) => lhs.Key.Hash.CompareTo(rhs.Key.Hash));
      return array;
    }
  }
}
