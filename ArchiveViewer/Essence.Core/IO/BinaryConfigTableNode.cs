// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigTableNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      BinaryConfigTableNode binaryConfigTableNode = new BinaryConfigTableNode(key);
      BinaryConfigTableNodeBase.ReadChildren(binaryReader, keyResolver, binaryConfigTableNode.Children);
      return binaryConfigTableNode;
    }

    protected internal override uint GetNodeType() => 100;

    protected override IEnumerable<BinaryConfigNode> GetOrderedChildren()
    {
      BinaryConfigNode[] array = this.Children.ToArray();
      Array.Sort<BinaryConfigNode>(array, (Comparison<BinaryConfigNode>) ((lhs, rhs) => lhs.Key.Hash.CompareTo(rhs.Key.Hash)));
      return (IEnumerable<BinaryConfigNode>) array;
    }
  }
}
