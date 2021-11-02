// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.BinaryConfigNode
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.IO;

namespace Essence.Core.IO
{
  public abstract class BinaryConfigNode
  {
    public BinaryConfigNode(string key)
      : this(new DictionaryKey(key))
    {
    }

    internal BinaryConfigNode(DictionaryKey key) => this.Key = key;

    public DictionaryKey Key { get; set; }

    protected internal abstract uint GetNodeType();

    protected internal abstract long Write(BinaryWriter binaryWriter);

    protected static void WritePadding(BinaryWriter binaryWriter, int alignment)
    {
      long num1 = (long) (alignment - 1);
      long num2 = (long) alignment - (binaryWriter.BaseStream.Position & num1) & num1;
      for (long index = 0; index < num2; ++index)
        binaryWriter.Write((byte) 0);
    }
  }
}
