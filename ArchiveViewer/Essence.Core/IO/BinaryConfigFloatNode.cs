using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigFloatNode : BinaryConfigNode
  {
    public const uint NodeType = 0;

    public BinaryConfigFloatNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigFloatNode(DictionaryKey key)
      : base(key)
    {
    }

    public float Value { get; set; }

    internal static BinaryConfigFloatNode Read(
      DictionaryKey key,
      BinaryReader binaryReader)
    {
      return new BinaryConfigFloatNode(key)
      {
        Value = binaryReader.ReadSingle()
      };
    }

    protected internal override uint GetNodeType() => 0;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      WritePadding(binaryWriter, 4);
      long position = binaryWriter.BaseStream.Position;
      binaryWriter.Write(Value);
      return position;
    }
  }
}
