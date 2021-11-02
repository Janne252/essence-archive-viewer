using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class BinaryConfigWStringNode : BinaryConfigStringNodeBase
  {
    public const uint NodeType = 4;

    public BinaryConfigWStringNode(string key)
      : base(key)
    {
    }

    internal BinaryConfigWStringNode(DictionaryKey key)
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
        var num1 = binaryReader.ReadByte();
        var num2 = binaryReader.ReadByte();
        if (num1 != 0 || num2 != 0)
        {
          if (byteList == null)
            byteList = new List<byte>(128);
          byteList.Add(num1);
          byteList.Add(num2);
        }
        else
          break;
      }
      var configStringNode = new BinaryConfigStringNode(key)
      {
          Value = byteList != null ? Encoding.Unicode.GetString(byteList.ToArray()) : string.Empty
      };
      return configStringNode;
    }

    protected internal override uint GetNodeType() => 4;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      WritePadding(binaryWriter, 2);
      var position = binaryWriter.BaseStream.Position;
      if (!string.IsNullOrEmpty(Value))
        binaryWriter.Write(Encoding.Unicode.GetBytes(Value));
      binaryWriter.Write((byte) 0);
      binaryWriter.Write((byte) 0);
      return position;
    }
  }
}
