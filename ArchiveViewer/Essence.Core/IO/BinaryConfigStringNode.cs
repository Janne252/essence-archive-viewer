// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigStringNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      if (!string.IsNullOrEmpty(this.Value))
        binaryWriter.Write(Chunky.Encoding.GetBytes(this.Value));
      binaryWriter.Write((byte) 0);
      return position;
    }
  }
}
