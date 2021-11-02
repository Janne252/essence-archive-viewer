// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigTableNodeBase
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public abstract class BinaryConfigTableNodeBase : BinaryConfigNode
  {
    public BinaryConfigTableNodeBase(string key)
      : base(key)
    {
    }

    internal BinaryConfigTableNodeBase(DictionaryKey key)
      : base(key)
    {
    }

    public List<BinaryConfigNode> Children { get; } = new List<BinaryConfigNode>();

    internal static void ReadChildren(
      BinaryReader binaryReader,
      KeyResolver keyResolver,
      List<BinaryConfigNode> children)
    {
      uint length = binaryReader.ReadUInt32();
      BinaryConfigTableNodeBase.NodeHeader[] nodeHeaderArray = new BinaryConfigTableNodeBase.NodeHeader[(int) length];
      for (uint index = 0; index < length; ++index)
      {
        nodeHeaderArray[(int) index].Key = binaryReader.ReadUInt64();
        nodeHeaderArray[(int) index].Type = binaryReader.ReadUInt32();
        nodeHeaderArray[(int) index].Offset = binaryReader.ReadUInt32();
      }
      children.Capacity = (int) length;
      long position = binaryReader.BaseStream.Position;
      foreach (BinaryConfigTableNodeBase.NodeHeader nodeHeader in nodeHeaderArray)
      {
        DictionaryKey key = keyResolver(nodeHeader.Key);
        binaryReader.BaseStream.Seek(position + (long) nodeHeader.Offset, SeekOrigin.Begin);
        switch (nodeHeader.Type)
        {
          case 0:
            children.Add((BinaryConfigNode) BinaryConfigFloatNode.Read(key, binaryReader));
            break;
          case 1:
            children.Add((BinaryConfigNode) BinaryConfigIntNode.Read(key, binaryReader));
            break;
          case 2:
            children.Add((BinaryConfigNode) BinaryConfigBoolNode.Read(key, binaryReader));
            break;
          case 3:
            children.Add((BinaryConfigNode) BinaryConfigStringNode.Read(key, binaryReader));
            break;
          case 4:
            children.Add((BinaryConfigNode) BinaryConfigWStringNode.Read(key, binaryReader));
            break;
          case 100:
            children.Add((BinaryConfigNode) BinaryConfigTableNode.Read(key, binaryReader, keyResolver));
            break;
          case 101:
            children.Add((BinaryConfigNode) BinaryConfigOrderedTableNode.Read(key, binaryReader, keyResolver));
            break;
          default:
            throw new IOException(string.Format("Unsupported node type [{0}].", (object) nodeHeader.Type));
        }
      }
    }

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      BinaryConfigNode.WritePadding(binaryWriter, 4);
      long position1 = binaryWriter.BaseStream.Position;
      binaryWriter.Write((uint) this.Children.Count);
      if (this.Children.Count > 0)
      {
        BinaryConfigTableNodeBase.NodeFixup[] nodeFixupArray = new BinaryConfigTableNodeBase.NodeFixup[this.Children.Count];
        int num1 = 0;
        foreach (BinaryConfigNode orderedChild in this.GetOrderedChildren())
        {
          binaryWriter.Write(orderedChild.Key.Hash);
          binaryWriter.Write(orderedChild.GetNodeType());
          nodeFixupArray[num1++] = new BinaryConfigTableNodeBase.NodeFixup(orderedChild, binaryWriter.BaseStream.Position);
          binaryWriter.Write(0U);
        }
        long position2 = binaryWriter.BaseStream.Position;
        for (int index = 0; index < nodeFixupArray.Length; ++index)
        {
          long num2 = nodeFixupArray[index].Node.Write(binaryWriter);
          nodeFixupArray[index] = new BinaryConfigTableNodeBase.NodeFixup(nodeFixupArray[index], (uint) (num2 - position2));
        }
        long position3 = binaryWriter.BaseStream.Position;
        for (int index = 0; index < nodeFixupArray.Length; ++index)
        {
          binaryWriter.BaseStream.Seek(nodeFixupArray[index].OffsetPosition, SeekOrigin.Begin);
          binaryWriter.Write(nodeFixupArray[index].Offset);
        }
        binaryWriter.BaseStream.Seek(position3, SeekOrigin.Begin);
      }
      return position1;
    }

    protected abstract IEnumerable<BinaryConfigNode> GetOrderedChildren();

    private struct NodeHeader
    {
      public ulong Key { get; set; }

      public uint Type { get; set; }

      public uint Offset { get; set; }
    }

    private struct NodeFixup
    {
      public NodeFixup(BinaryConfigNode node, long offsetPosition)
      {
        this.Node = node;
        this.OffsetPosition = offsetPosition;
        this.Offset = 0U;
      }

      public NodeFixup(BinaryConfigTableNodeBase.NodeFixup nodeFixup, uint offset)
      {
        this.Node = nodeFixup.Node;
        this.OffsetPosition = nodeFixup.OffsetPosition;
        this.Offset = offset;
      }

      public BinaryConfigNode Node { get; }

      public long OffsetPosition { get; }

      public uint Offset { get; }
    }
  }
}
