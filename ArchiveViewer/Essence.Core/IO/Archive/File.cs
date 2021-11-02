// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Archive.File
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;

namespace Essence.Core.IO.Archive
{
  public sealed class File : INode
  {
    private uint? m_crc32;

    internal File(
      Essence.Core.IO.Archive.Archive archive,
      string name,
      uint storeLength,
      uint length,
      FileVerificationType verificationType,
      FileStorageType storageType,
      long fileOffset,
      uint? crc32)
    {
      this.Archive = archive;
      this.Name = name;
      int num = name.LastIndexOf('.');
      this.Extension = num >= 0 ? name.Substring(num + 1) : string.Empty;
      this.StoreLength = storeLength;
      this.Length = length;
      this.VerificationType = verificationType;
      this.StorageType = storageType;
      this.FileOffset = fileOffset;
      this.m_crc32 = crc32;
    }

    public Essence.Core.IO.Archive.Archive Archive { get; }

    public INode Parent { get; internal set; }

    public IReadOnlyList<INode> Children => (IReadOnlyList<INode>) null;

    public string Name { get; }

    public string FullName => this.Parent == null ? this.Name : this.Parent.FullName + this.Name;

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
        if (!this.m_crc32.HasValue)
          this.m_crc32 = new uint?(this.Archive.GetCRC(this));
        return this.m_crc32.Value;
      }
    }

    public byte[] GetData() => this.Archive.GetData(this);

    public override string ToString() => this.Name;
  }
}
