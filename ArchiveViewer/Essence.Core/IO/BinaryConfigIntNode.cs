// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigIntNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      BinaryConfigNode.WritePadding(binaryWriter, 4);
      long position = binaryWriter.BaseStream.Position;
      binaryWriter.Write(this.Value);
      return position;
    }
  }
}
