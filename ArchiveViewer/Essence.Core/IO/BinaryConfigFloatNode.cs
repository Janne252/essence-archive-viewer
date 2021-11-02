// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigFloatNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      BinaryConfigNode.WritePadding(binaryWriter, 4);
      long position = binaryWriter.BaseStream.Position;
      binaryWriter.Write(this.Value);
      return position;
    }
  }
}
