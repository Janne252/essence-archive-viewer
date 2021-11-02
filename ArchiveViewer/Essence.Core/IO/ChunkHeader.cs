// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.ChunkHeader
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

namespace Essence.Core.IO
{
  public struct ChunkHeader
  {
    public ChunkHeader(FourCC type, FourCC id, uint version, uint size, string name)
    {
      this.Type = type;
      this.ID = id;
      this.Version = version;
      this.Size = size;
      this.Name = name;
    }

    public FourCC Type { get; }

    public FourCC ID { get; }

    public uint Version { get; }

    public uint Size { get; }

    public string Name { get; }
  }
}
