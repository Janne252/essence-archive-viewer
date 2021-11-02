namespace Essence.Core.IO
{
  public struct ChunkHeader
  {
    public ChunkHeader(FourCC type, FourCC id, uint version, uint size, string name)
    {
      Type = type;
      ID = id;
      Version = version;
      Size = size;
      Name = name;
    }

    public FourCC Type { get; }

    public FourCC ID { get; }

    public uint Version { get; }

    public uint Size { get; }

    public string Name { get; }
  }
}
