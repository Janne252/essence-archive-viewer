using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Essence.Core.IO.Archive
{
  public sealed class Archive : IDisposable, INode
  {
    private static readonly byte[] Magic = new byte[8]
    {
      (byte) 95,
      (byte) 65,
      (byte) 82,
      (byte) 67,
      (byte) 72,
      (byte) 73,
      (byte) 86,
      (byte) 69
    };
    private const int MD5Length = 16;
    private FileStream m_fileStream;
    private BinaryReader m_reader;

    public Archive(string fileName)
    {
      FileInfo fileInfo = new FileInfo(fileName);
      Name = fileInfo.Name;
      FullName = fileInfo.FullName;
      m_fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      Length = m_fileStream.Length;
      m_reader = new BinaryReader((Stream) m_fileStream);
      ReadHeader(m_reader);
    }

    ~Archive() => Dispose(false);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_fileStream == null)
        return;
      m_reader.Close();
      m_fileStream = (FileStream) null;
    }

    private Func<INode, bool> GetSearchPredicate(
      string[] parts,
      int index,
      SearchMethod searchMethod,
      bool matchCase)
    {
      string part = parts[index];
      Type type;
      if (index == 0)
      {
        part = part.TrimEnd(':');
        type = typeof (TOC);
      }
      else
        type = index + 1 != parts.Length ? typeof (Folder) : typeof (File);
      switch (searchMethod)
      {
        case SearchMethod.Substring:
          StringComparison stringComparison1 = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
          return (Func<INode, bool>) (n => type.IsInstanceOfType((object) n) && n.Name.IndexOf(part, stringComparison1) >= 0);
        case SearchMethod.Wildcards:
          if (part.IndexOfAny(new char[2]{ '*', '?' }) == -1)
          {
            StringComparison stringComparison2 = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            return (Func<INode, bool>) (n => type.IsInstanceOfType((object) n) && n.Name.Equals(part, stringComparison2));
          }
          Regex regx1 = Wildcard.CreateWildcardRegex(part, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
          return (Func<INode, bool>) (n => type.IsInstanceOfType((object) n) && regx1.IsMatch(n.Name));
        case SearchMethod.RegularExpression:
          Regex regx2 = new Regex(part, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
          return (Func<INode, bool>) (n => type.IsInstanceOfType((object) n) && regx2.IsMatch(n.Name));
        default:
          return (Func<INode, bool>) null;
      }
    }

    public List<File> GetFiles(
      string pattern,
      SearchMethod searchMethod,
      bool matchCase)
    {
      string[] parts = pattern.Split(new char[2]
      {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
      }, StringSplitOptions.RemoveEmptyEntries);
      Func<INode, bool>[] searchPredicates = new Func<INode, bool>[parts.Length];
      for (int index = 0; index < parts.Length; ++index)
        searchPredicates[index] = GetSearchPredicate(parts, index, searchMethod, matchCase);
      List<File> files = new List<File>();
      if (searchPredicates.Length != 0)
        GetFiles(searchPredicates, 0, Children, files);
      return files;
    }

    private void GetFiles(
      Func<INode, bool>[] searchPredicates,
      int index,
      IReadOnlyList<INode> children,
      List<File> files)
    {
      if (children == null)
        return;
      Func<INode, bool> searchPredicate = searchPredicates[index];
      foreach (INode child in (IEnumerable<INode>) children)
      {
        if (searchPredicate(child))
        {
          if (searchPredicates.Length == index + 1)
            files.Add((File) child);
          else
            GetFiles(searchPredicates, index + 1, child.Children, files);
        }
      }
    }

    internal uint GetCRC(File file)
    {
      if (file == null)
        throw new ArgumentNullException(nameof (file));
      Exception innerException = (Exception) null;
      try
      {
        long offset = file.FileOffset - 4L;
        if (m_fileStream.Seek(offset, SeekOrigin.Begin) == offset)
          return m_reader.ReadUInt32();
      }
      catch (Exception ex)
      {
        innerException = ex;
      }
      throw new ApplicationException(string.Format("Error reading CRC for file {0}.", (object) file), innerException);
    }

    internal byte[] GetData(File file)
    {
      if (file == null)
        throw new ArgumentNullException(nameof (file));
      Exception innerException = (Exception) null;
      try
      {
        if (m_fileStream.Seek(file.FileOffset, SeekOrigin.Begin) == file.FileOffset)
        {
          switch (file.StorageType)
          {
            case FileStorageType.Store:
              byte[] buffer1 = new byte[(int) file.StoreLength];
              if (m_fileStream.Read(buffer1, 0, buffer1.Length) == buffer1.Length)
                return buffer1;
              throw new ApplicationException(string.Format("Unable to read {0} bytes at {1}.", (object) file.StoreLength, (object) file.FileOffset));
            case FileStorageType.StreamCompress:
            case FileStorageType.BufferCompress:
              if (file.StoreLength > 0U)
              {
                m_fileStream.Seek(2L, SeekOrigin.Current);
                using (DeflateStream deflateStream = new DeflateStream((Stream) m_fileStream, CompressionMode.Decompress, true))
                {
                  byte[] buffer2 = new byte[(int) file.StoreLength];
                  if (deflateStream.Read(buffer2, 0, buffer2.Length) == buffer2.Length)
                    return buffer2;
                }
              }
              throw new ApplicationException(string.Format("Unable to deflate {0} bytes at {1}.", (object) file.StoreLength, (object) file.FileOffset));
            default:
              throw new ApplicationException(string.Format("Unsupported stream type {0}.", (object) file.StorageType));
          }
        }
      }
      catch (Exception ex)
      {
        innerException = ex;
      }
      throw new ApplicationException(string.Format("Error reading data for file {0}.", (object) file), innerException);
    }

    public override string ToString() => Name;

    Archive INode.Archive => this;

    public INode Parent => (INode) null;

    public IReadOnlyList<INode> Children { get; private set; }

    public string Name { get; }

    public string FullName { get; }

    public long Length { get; private set; }

    public ushort Version { get; private set; }

    public Product Product { get; private set; }

    public string NiceName { get; private set; }

    public byte[] FileMD5 { get; private set; }

    public byte[] HeaderMD5 { get; private set; }

    public uint BlockSize { get; private set; }

    private uint ReadIndex(BinaryReader reader) => Version <= (ushort) 4 ? (uint) reader.ReadUInt16() : reader.ReadUInt32();

    private string ReadFixedString(BinaryReader reader, int charCount, int charSize)
    {
      byte[] bytes = reader.ReadBytes(charCount * charSize);
      if (bytes == null || bytes.Length != charCount * charSize)
        throw new ApplicationException(string.Format("File length is not sufficient for fixed string of length {0}.", (object) (charCount * charSize)));
      for (int index1 = 0; index1 < charCount; ++index1)
      {
        bool flag = true;
        for (int index2 = 0; index2 < charSize; ++index2)
        {
          if (bytes[index1 * charSize + index2] != (byte) 0)
          {
            flag = false;
            break;
          }
        }
        if (flag)
          charCount = index1;
      }
      if (charSize == 1)
        return Encoding.UTF8.GetString(bytes, 0, charCount * charSize);
      if (charSize == 2)
        return Encoding.Unicode.GetString(bytes, 0, charCount * charSize);
      throw new ArgumentOutOfRangeException(nameof (charSize));
    }

    private string ReadDynamicString(BinaryReader reader, long offset)
    {
      if (m_fileStream.Seek(offset, SeekOrigin.Begin) != offset)
        return (string) null;
      List<byte> byteList = new List<byte>(64);
      while (true)
      {
        byte num = reader.ReadByte();
        if (num != (byte) 0)
          byteList.Add(num);
        else
          break;
      }
      return Encoding.ASCII.GetString(byteList.ToArray());
    }

    private void ReadHeader(BinaryReader reader)
    {
      byte[] first = reader.ReadBytes(Magic.Length);
      if (first == null || first.Length != Magic.Length || !((IEnumerable<byte>) first).SequenceEqual<byte>((IEnumerable<byte>) Magic))
        throw new ApplicationException("Magic does not match archive file magic.");
      Version = reader.ReadUInt16();
      Product = (Product) reader.ReadUInt16();
      if (Version < (ushort) 4 || Version > (ushort) 10 || Product != Product.Essence)
        throw new ApplicationException(string.Format("Version {0} of product {1} is not supported.", (object) Version, (object) Product));
      if (Version < (ushort) 6)
      {
        FileMD5 = reader.ReadBytes(16);
        if (FileMD5 == null || FileMD5.Length != 16)
          throw new ApplicationException("Header length is not sufficient for file MD5.");
      }
      NiceName = ReadFixedString(reader, 64, 2);
      if (Version < (ushort) 6)
      {
        HeaderMD5 = reader.ReadBytes(16);
        if (HeaderMD5 == null || HeaderMD5.Length != 16)
          throw new ApplicationException("Header length is not sufficient for header MD5.");
      }
      long? nullable1 = new long?();
      if (Version >= (ushort) 9)
        nullable1 = new long?((long) reader.ReadUInt64());
      else if (Version >= (ushort) 8)
        nullable1 = new long?((long) reader.ReadUInt32());
      uint num1 = reader.ReadUInt32();
      long? nullable2 = new long?();
      long dataOffset;
      if (Version >= (ushort) 9)
      {
        dataOffset = (long) reader.ReadUInt64();
        nullable2 = new long?((long) reader.ReadUInt64());
      }
      else
      {
        dataOffset = (long) reader.ReadUInt32();
        if (Version >= (ushort) 8)
          nullable2 = new long?((long) reader.ReadUInt32());
      }
      int num2 = (int) reader.ReadUInt32();
      if (Version >= (ushort) 8)
        m_fileStream.Seek(256L, SeekOrigin.Current);
      long offset = nullable1.HasValue ? nullable1.Value : m_fileStream.Position;
      if (offset + (long) num1 > Length)
        throw new ApplicationException(string.Format("Header blob [{0} B, {1} B] outside of file length {1} B.", (object) offset, (object) (offset + (long) num1), (object) Length));
      if (nullable2.HasValue && dataOffset + nullable2.Value > Length)
        throw new ApplicationException(string.Format("Data blob [{0} B, {1} B] outside of file length {1} B.", (object) dataOffset, (object) (dataOffset + nullable2.Value), (object) Length));
      if (m_fileStream.Seek(offset, SeekOrigin.Begin) != offset)
        throw new ApplicationException(string.Format("Unable to seek to position {0} B.", (object) offset));
      uint num3 = reader.ReadUInt32();
      uint length1 = ReadIndex(reader);
      uint num4 = reader.ReadUInt32();
      uint length2 = ReadIndex(reader);
      uint num5 = reader.ReadUInt32();
      uint length3 = ReadIndex(reader);
      uint num6 = reader.ReadUInt32();
      int num7 = (int) ReadIndex(reader);
      if (Version >= (ushort) 7)
      {
        int num8 = (int) reader.ReadUInt32();
        if (Version >= (ushort) 8)
        {
          int num9 = (int) reader.ReadUInt32();
        }
        BlockSize = reader.ReadUInt32();
      }
      if (m_fileStream.Seek(offset + (long) num3, SeekOrigin.Begin) != offset + (long) num3)
        throw new ApplicationException(string.Format("Unable to seek to position {0} B.", (object) (offset + (long) num3)));
      TOCData[] tocDataArray = new TOCData[(int) length1];
      for (uint index = 0; index < length1; ++index)
      {
        tocDataArray[(int) index].alias = ReadFixedString(reader, 64, 1);
        tocDataArray[(int) index].name = ReadFixedString(reader, 64, 1);
        tocDataArray[(int) index].folderStartIndex = ReadIndex(reader);
        tocDataArray[(int) index].folderEndIndex = ReadIndex(reader);
        tocDataArray[(int) index].fileStartIndex = ReadIndex(reader);
        tocDataArray[(int) index].fileEndIndex = ReadIndex(reader);
        tocDataArray[(int) index].folderRootIndex = ReadIndex(reader);
      }
      if (m_fileStream.Seek(offset + (long) num4, SeekOrigin.Begin) != offset + (long) num4)
        throw new ApplicationException(string.Format("Unable to seek to position {0} B.", (object) (offset + (long) num4)));
      FolderData[] folderData = new FolderData[(int) length2];
      for (uint index = 0; index < length2; ++index)
      {
        folderData[(int) index].nameOffset = reader.ReadUInt32();
        folderData[(int) index].folderStartIndex = ReadIndex(reader);
        folderData[(int) index].folderEndIndex = ReadIndex(reader);
        folderData[(int) index].fileStartIndex = ReadIndex(reader);
        folderData[(int) index].fileEndIndex = ReadIndex(reader);
      }
      if (m_fileStream.Seek(offset + (long) num5, SeekOrigin.Begin) != offset + (long) num5)
        throw new ApplicationException(string.Format("Unable to seek to position {0} B.", (object) (offset + (long) num5)));
      FileData[] fileData = new FileData[(int) length3];
      for (uint index = 0; index < length3; ++index)
      {
        fileData[(int) index].nameOffset = reader.ReadUInt32();
        if (Version >= (ushort) 8)
          fileData[(int) index].hashOffset = reader.ReadUInt32();
        fileData[(int) index].dataOffset = Version < (ushort) 9 ? (long) reader.ReadUInt32() : (long) reader.ReadUInt64();
        fileData[(int) index].length = reader.ReadUInt32();
        fileData[(int) index].storeLength = reader.ReadUInt32();
        if (Version < (ushort) 10)
        {
          int num10 = (int) reader.ReadUInt32();
        }
        fileData[(int) index].verificationType = (FileVerificationType) reader.ReadByte();
        fileData[(int) index].storageType = (FileStorageType) reader.ReadByte();
        if (Version >= (ushort) 6)
          fileData[(int) index].crc = reader.ReadUInt32();
        if (Version == (ushort) 7)
          fileData[(int) index].hashOffset = reader.ReadUInt32();
      }
      List<INode> list = new List<INode>(tocDataArray.Length);
      foreach (TOCData tocData in tocDataArray)
      {
        IList<INode> children = CreateChildren(reader, folderData, tocData.folderRootIndex, tocData.folderRootIndex + 1U, fileData, 0U, 0U, offset + (long) num6, dataOffset);
        TOC parent = new TOC(this, children, tocData.alias, tocData.name);
        SetParent((INode) parent, children);
        list.Add((INode) parent);
      }
      Children = (IReadOnlyList<INode>) new ReadOnlyCollection<INode>((IList<INode>) list);
    }

    private IList<INode> CreateChildren(
      BinaryReader reader,
      FolderData[] folderData,
      uint folderStartIndex,
      uint folderEndIndex,
      FileData[] fileData,
      uint fileStartIndex,
      uint fileEndIndex,
      long stringOffset,
      long dataOffset)
    {
      List<INode> children1 = new List<INode>((int) folderEndIndex - (int) folderStartIndex + ((int) fileEndIndex - (int) fileStartIndex));
      for (uint index = folderStartIndex; index < folderEndIndex; ++index)
      {
        string name = ReadDynamicString(reader, stringOffset + (long) folderData[(int) index].nameOffset) ?? string.Format("Folder_{0}", (object) index);
        int num = name.LastIndexOfAny(new char[2]
        {
          Path.DirectorySeparatorChar,
          Path.AltDirectorySeparatorChar
        });
        if (num >= 0)
          name = name.Substring(num + 1);
        IList<INode> children2 = CreateChildren(reader, folderData, folderData[(int) index].folderStartIndex, folderData[(int) index].folderEndIndex, fileData, folderData[(int) index].fileStartIndex, folderData[(int) index].fileEndIndex, stringOffset, dataOffset);
        if (name.Length > 0)
        {
          Folder parent = new Folder(this, children2, name);
          SetParent((INode) parent, children2);
          children1.Add((INode) parent);
        }
        else
          children1.AddRange((IEnumerable<INode>) children2);
      }
      for (uint index = fileStartIndex; index < fileEndIndex; ++index)
      {
        long fileOffset = dataOffset + fileData[(int) index].dataOffset;
        string name = ReadDynamicString(reader, stringOffset + (long) fileData[(int) index].nameOffset);
        if (name == null && Version < (ushort) 6)
        {
          long offset = fileOffset - 260L;
          name = m_fileStream.Seek(offset, SeekOrigin.Begin) != offset ? string.Format("File_{0}.dat", (object) index) : ReadFixedString(reader, 256, 1);
        }
        uint? crc32 = new uint?();
        if (Version >= (ushort) 6)
          crc32 = new uint?(fileData[(int) index].crc);
        children1.Add((INode) new File(this, name, fileData[(int) index].storeLength, fileData[(int) index].length, fileData[(int) index].verificationType, fileData[(int) index].storageType, fileOffset, crc32));
      }
      return (IList<INode>) children1;
    }

    private static void SetParent(INode parent, IList<INode> children)
    {
      foreach (INode child in (IEnumerable<INode>) children)
      {
        if (!(child is Folder folder))
        {
          if (child is File file)
            file.Parent = parent;
        }
        else
          folder.Parent = parent;
      }
    }

    public enum SearchMethod
    {
      Substring,
      Wildcards,
      RegularExpression,
    }

    private struct TOCData
    {
      public string alias;
      public string name;
      public uint folderStartIndex;
      public uint folderEndIndex;
      public uint fileStartIndex;
      public uint fileEndIndex;
      public uint folderRootIndex;
    }

    private struct FolderData
    {
      public uint nameOffset;
      public uint folderStartIndex;
      public uint folderEndIndex;
      public uint fileStartIndex;
      public uint fileEndIndex;
    }

    private struct FileData
    {
      public uint nameOffset;
      public long dataOffset;
      public uint length;
      public uint storeLength;
      public FileVerificationType verificationType;
      public FileStorageType storageType;
      public uint crc;
      public uint hashOffset;
    }
  }
}
