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
      List<byte> byteList = (List<byte>) null;
      while (true)
      {
        byte num1 = binaryReader.ReadByte();
        byte num2 = binaryReader.ReadByte();
        if (num1 != (byte) 0 || num2 != (byte) 0)
        {
          if (byteList == null)
            byteList = new List<byte>(128);
          byteList.Add(num1);
          byteList.Add(num2);
        }
        else
          break;
      }
      BinaryConfigStringNode configStringNode = new BinaryConfigStringNode(key);
      configStringNode.Value = byteList != null ? Encoding.Unicode.GetString(byteList.ToArray()) : string.Empty;
      return configStringNode;
    }

    protected internal override uint GetNodeType() => 4;

    protected internal override long Write(BinaryWriter binaryWriter)
    {
      WritePadding(binaryWriter, 2);
      long position = binaryWriter.BaseStream.Position;
      if (!string.IsNullOrEmpty(Value))
        binaryWriter.Write(Encoding.Unicode.GetBytes(Value));
      binaryWriter.Write((byte) 0);
      binaryWriter.Write((byte) 0);
      return position;
    }
  }
}
