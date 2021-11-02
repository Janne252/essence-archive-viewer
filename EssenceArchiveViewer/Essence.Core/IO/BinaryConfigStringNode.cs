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
      var byteList = (List<byte>) null;
      while (true)
      {
        var num = binaryReader.ReadByte();
        if (num != 0)
        {
          if (byteList == null)
            byteList = new List<byte>(64);
          byteList.Add(num);
        }
        else
          break;
      }
      var configStringNode = new BinaryConfigStringNode(key)
      {
          Value = byteList != null ? Chunky.Encoding.GetString(byteList.ToArray()) : string.Empty
      };
      return configStringNode;
    }

    protected internal override uint GetNodeType() => 3;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      var position = binaryWriter.BaseStream.Position;
      if (!string.IsNullOrEmpty(Value))
        binaryWriter.Write(Chunky.Encoding.GetBytes(Value));
      binaryWriter.Write((byte) 0);
      return position;
    }
  }
}
