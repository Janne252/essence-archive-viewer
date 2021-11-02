using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigBoolNode : BinaryConfigNode
  {
    public const uint NodeType = 2;

    public BinaryConfigBoolNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigBoolNode(DictionaryKey key)
      : base(key)
    {
    }

    public bool Value { get; set; }

    internal static BinaryConfigBoolNode Read(
      DictionaryKey key,
      BinaryReader binaryReader)
    {
      return new BinaryConfigBoolNode(key)
      {
        Value = binaryReader.ReadBoolean()
      };
    }

    protected internal override uint GetNodeType() => 2;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      long position = binaryWriter.BaseStream.Position;
      binaryWriter.Write(Value);
      return position;
    }
  }
}
