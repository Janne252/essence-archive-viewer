// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigBoolNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      binaryWriter.Write(this.Value);
      return position;
    }
  }
}
