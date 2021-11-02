using System.Collections.Generic;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigStringNode : BinaryConfigStringNodeBase
  {
    public const uint NodeType = 3;

    public BinaryConfigStringNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigStringNode(DictionaryKey key)
      : base(key)
    {
    }

    internal static BinaryConfigStringNode Read(
      DictionaryKey key,
      BinaryReader binaryReader)
    {
      List<byte> byteList = (List<byte>) null;
      while (true)
      {
        byte num = binaryReader.ReadByte();
        if (num != (byte) 0)
        {
          if (byteList == null)
            byteList = new List<byte>(64);
          byteList.Add(num);
        }
        else
          break;
      }
      BinaryConfigStringNode configStringNode = new BinaryConfigStringNode(key);
      configStringNode.Value = byteList != null ? Chunky.Encoding.GetString(byteList.ToArray()) : string.Empty;
      return configStringNode;
    }

    protected internal override uint GetNodeType() => 3;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      long position = binaryWriter.BaseStream.Position;
      if (!string.IsNullOrEmpty(Value))
        binaryWriter.Write(Chunky.Encoding.GetBytes(Value));
      binaryWriter.Write((byte) 0);
      return position;
    }
  }
}
