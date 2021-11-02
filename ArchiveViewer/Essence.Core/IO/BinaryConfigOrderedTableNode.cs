// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigOrderedTableNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      BinaryConfigOrderedTableNode orderedTableNode = new BinaryConfigOrderedTableNode(key);
      BinaryConfigTableNodeBase.ReadChildren(binaryReader, keyResolver, orderedTableNode.Children);
      return orderedTableNode;
    }

    protected internal override uint GetNodeType() => 101;

    protected override IEnumerable<BinaryConfigNode> GetOrderedChildren() => (IEnumerable<BinaryConfigNode>) this.Children;
  }
}
