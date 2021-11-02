using System.Collections.Generic;

namespace Essence.Core.IO.Archive
{
  public sealed class File : INode
  {
    private uint? m_crc32;

    internal File(
      Archive archive,
      string name,
      uint storeLength,
      uint length,
      FileVerificationType verificationType,
      FileStorageType storageType,
      long fileOffset,
      uint? crc32)
    {
      Archive = archive;
      Name = name;
      var num = name.LastIndexOf('.');
      Extension = num >= 0 ? name.Substring(num + 1) : string.Empty;
      StoreLength = storeLength;
      Length = length;
      VerificationType = verificationType;
      StorageType = storageType;
      FileOffset = fileOffset;
      m_crc32 = crc32;
    }

    public Archive Archive { get; }

    public INode Parent { get; internal set; }

    public IReadOnlyList<INode> Children => null;

    public string Name { get; }

    public string FullName => Parent == null ? Name : Parent.FullName + Name;

    public string Extension { get; }

    public uint StoreLength { get; }

    public uint Length { get; }

    public FileVerificationType VerificationType { get; }

    public FileStorageType StorageType { get; }

    public long FileOffset { get; }

    public uint CRC32
    {
      get
      {
        if (!m_crc32.HasValue)
          m_crc32 = Archive.GetCRC(this);
        return m_crc32.Value;
      }
    }

    public byte[] GetData() => Archive.GetData(this);

    public override string ToString() => Name;
  }
}
