using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigIntNode : BinaryConfigNode
  {
    public const uint NodeType = 1;

    public BinaryConfigIntNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigIntNode(DictionaryKey key)
      : base(key)
    {
    }

    public int Value { get; set; }

    internal static BinaryConfigIntNode Read(
      DictionaryKey key,
      BinaryReader binaryReader)
    {
      return new BinaryConfigIntNode(key)
      {
        Value = binaryReader.ReadInt32()
      };
    }

    protected internal override uint GetNodeType() => 1;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      WritePadding(binaryWriter, 4);
      var position = binaryWriter.BaseStream.Position;
      binaryWriter.Write(Value);
      return position;
    }
  }
}
