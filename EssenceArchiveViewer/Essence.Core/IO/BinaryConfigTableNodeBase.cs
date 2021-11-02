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

    public List<BinaryConfigNode> Children { get; } = new();

    internal static void ReadChildren(
      BinaryReader binaryReader,
      KeyResolver keyResolver,
      List<BinaryConfigNode> children)
    {
      var length = binaryReader.ReadUInt32();
      var nodeHeaderArray = new NodeHeader[(int) length];
      for (uint index = 0; index < length; ++index)
      {
        nodeHeaderArray[(int) index].Key = binaryReader.ReadUInt64();
        nodeHeaderArray[(int) index].Type = binaryReader.ReadUInt32();
        nodeHeaderArray[(int) index].Offset = binaryReader.ReadUInt32();
      }
      children.Capacity = (int) length;
      var position = binaryReader.BaseStream.Position;
      foreach (var nodeHeader in nodeHeaderArray)
      {
        var key = keyResolver(nodeHeader.Key);
        binaryReader.BaseStream.Seek(position + nodeHeader.Offset, SeekOrigin.Begin);
        switch (nodeHeader.Type)
        {
          case 0:
            children.Add(BinaryConfigFloatNode.Read(key, binaryReader));
            break;
          case 1:
            children.Add(BinaryConfigIntNode.Read(key, binaryReader));
            break;
          case 2:
            children.Add(BinaryConfigBoolNode.Read(key, binaryReader));
            break;
          case 3:
            children.Add(BinaryConfigStringNode.Read(key, binaryReader));
            break;
          case 4:
            children.Add(BinaryConfigWStringNode.Read(key, binaryReader));
            break;
          case 100:
            children.Add(BinaryConfigTableNode.Read(key, binaryReader, keyResolver));
            break;
          case 101:
            children.Add(BinaryConfigOrderedTableNode.Read(key, binaryReader, keyResolver));
            break;
          default:
            throw new IOException($"Unsupported node type [{nodeHeader.Type}].");
        }
      }
    }

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      WritePadding(binaryWriter, 4);
      var position1 = binaryWriter.BaseStream.Position;
      binaryWriter.Write((uint) Children.Count);
      if (Children.Count > 0)
      {
        var nodeFixupArray = new NodeFixup[Children.Count];
        var num1 = 0;
        foreach (var orderedChild in GetOrderedChildren())
        {
          binaryWriter.Write(orderedChild.Key.Hash);
          binaryWriter.Write(orderedChild.GetNodeType());
          nodeFixupArray[num1++] = new NodeFixup(orderedChild, binaryWriter.BaseStream.Position);
          binaryWriter.Write(0U);
        }
        var position2 = binaryWriter.BaseStream.Position;
        for (var index = 0; index < nodeFixupArray.Length; ++index)
        {
          var num2 = nodeFixupArray[index].Node.Write(binaryWriter);
          nodeFixupArray[index] = new NodeFixup(nodeFixupArray[index], (uint) (num2 - position2));
        }
        var position3 = binaryWriter.BaseStream.Position;
        for (var index = 0; index < nodeFixupArray.Length; ++index)
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
        Node = node;
        OffsetPosition = offsetPosition;
        Offset = 0U;
      }

      public NodeFixup(NodeFixup nodeFixup, uint offset)
      {
        Node = nodeFixup.Node;
        OffsetPosition = nodeFixup.OffsetPosition;
        Offset = offset;
      }

      public BinaryConfigNode Node { get; }

      public long OffsetPosition { get; }

      public uint Offset { get; }
    }
  }
}
