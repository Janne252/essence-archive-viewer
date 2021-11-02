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
    private static readonly byte[] Magic = {
      95,
      65,
      82,
      67,
      72,
      73,
      86,
      69
    };

    private FileStream m_fileStream;
    private readonly BinaryReader m_reader;

    public Archive(string fileName)
    {
      var fileInfo = new FileInfo(fileName);
      Name = fileInfo.Name;
      FullName = fileInfo.FullName;
      m_fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      Length = m_fileStream.Length;
      m_reader = new BinaryReader(m_fileStream);
      ReadHeader(m_reader);
    }

    ~Archive() => Dispose(false);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_fileStream == null)
        return;
      m_reader.Close();
      m_fileStream = null;
    }

    private Func<INode, bool> GetSearchPredicate(
      string[] parts,
      int index,
      SearchMethod searchMethod,
      bool matchCase)
    {
      var part = parts[index];
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
          var stringComparison1 = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
          return n => type.IsInstanceOfType(n) && n.Name.IndexOf(part, stringComparison1) >= 0;
        case SearchMethod.Wildcards:
          if (part.IndexOfAny(new char[2]{ '*', '?' }) == -1)
          {
            var stringComparison2 = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            return n => type.IsInstanceOfType(n) && n.Name.Equals(part, stringComparison2);
          }
          var regx1 = Wildcard.CreateWildcardRegex(part, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
          return n => type.IsInstanceOfType(n) && regx1.IsMatch(n.Name);
        case SearchMethod.RegularExpression:
          var regx2 = new Regex(part, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
          return n => type.IsInstanceOfType(n) && regx2.IsMatch(n.Name);
        default:
          return null;
      }
    }

    public List<File> GetFiles(
      string pattern,
      SearchMethod searchMethod,
      bool matchCase)
    {
      var parts = pattern.Split(new char[2]
      {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
      }, StringSplitOptions.RemoveEmptyEntries);
      var searchPredicates = new Func<INode, bool>[parts.Length];
      for (var index = 0; index < parts.Length; ++index)
        searchPredicates[index] = GetSearchPredicate(parts, index, searchMethod, matchCase);
      var files = new List<File>();
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
      var searchPredicate = searchPredicates[index];
      foreach (var child in children)
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
      var innerException = (Exception) null;
      try
      {
        var offset = file.FileOffset - 4L;
        if (m_fileStream.Seek(offset, SeekOrigin.Begin) == offset)
          return m_reader.ReadUInt32();
      }
      catch (Exception ex)
      {
        innerException = ex;
      }
      throw new ApplicationException($"Error reading CRC for file {file}.", innerException);
    }

    internal byte[] GetData(File file)
    {
      if (file == null)
        throw new ArgumentNullException(nameof (file));
      var innerException = (Exception) null;
      try
      {
        if (m_fileStream.Seek(file.FileOffset, SeekOrigin.Begin) == file.FileOffset)
        {
          switch (file.StorageType)
          {
            case FileStorageType.Store:
              var buffer1 = new byte[(int) file.StoreLength];
              if (m_fileStream.Read(buffer1, 0, buffer1.Length) == buffer1.Length)
                return buffer1;
              throw new ApplicationException($"Unable to read {file.StoreLength} bytes at {file.FileOffset}.");
            case FileStorageType.StreamCompress:
            case FileStorageType.BufferCompress:
              if (file.StoreLength > 0U)
              {
                m_fileStream.Seek(2L, SeekOrigin.Current);
                using var deflateStream = new DeflateStream(m_fileStream, CompressionMode.Decompress, true);
                var buffer2 = new byte[(int) file.StoreLength];
                if (deflateStream.Read(buffer2, 0, buffer2.Length) == buffer2.Length)
                    return buffer2;
              }
              throw new ApplicationException($"Unable to deflate {file.StoreLength} bytes at {file.FileOffset}.");
            default:
              throw new ApplicationException($"Unsupported stream type {file.StorageType}.");
          }
        }
      }
      catch (Exception ex)
      {
        innerException = ex;
      }
      throw new ApplicationException($"Error reading data for file {file}.", innerException);
    }

    public override string ToString() => Name;

    Archive INode.Archive => this;

    public INode Parent => null;

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

    private uint ReadIndex(BinaryReader reader) => Version <= 4 ? reader.ReadUInt16() : reader.ReadUInt32();

    private string ReadFixedString(BinaryReader reader, int charCount, int charSize)
    {
      var bytes = reader.ReadBytes(charCount * charSize);
      if (bytes == null || bytes.Length != charCount * charSize)
        throw new ApplicationException(
            $"File length is not sufficient for fixed string of length {charCount * charSize}.");
      for (var index1 = 0; index1 < charCount; ++index1)
      {
        var flag = true;
        for (var index2 = 0; index2 < charSize; ++index2)
        {
          if (bytes[index1 * charSize + index2] != 0)
          {
            flag = false;
            break;
          }
        }
        if (flag)
          charCount = index1;
      }

      return charSize switch
      {
          1 => Encoding.UTF8.GetString(bytes, 0, charCount * charSize),
          2 => Encoding.Unicode.GetString(bytes, 0, charCount * charSize),
          _ => throw new ArgumentOutOfRangeException(nameof(charSize))
      };
    }

    private string ReadDynamicString(BinaryReader reader, long offset)
    {
      if (m_fileStream.Seek(offset, SeekOrigin.Begin) != offset)
        return null;
      var byteList = new List<byte>(64);
      while (true)
      {
        var num = reader.ReadByte();
        if (num != 0)
          byteList.Add(num);
        else
          break;
      }
      return Encoding.ASCII.GetString(byteList.ToArray());
    }

    private void ReadHeader(BinaryReader reader)
    {
      var first = reader.ReadBytes(Magic.Length);
      if (first == null || first.Length != Magic.Length || !first.SequenceEqual<byte>(Magic))
        throw new ApplicationException("Magic does not match archive file magic.");
      Version = reader.ReadUInt16();
      Product = (Product) reader.ReadUInt16();
      if (Version is < 4 or > 10 || Product != Product.Essence)
        throw new ApplicationException($"Version {Version} of product {Product} is not supported.");
      if (Version < 6)
      {
        FileMD5 = reader.ReadBytes(16);
        if (FileMD5 == null || FileMD5.Length != 16)
          throw new ApplicationException("Header length is not sufficient for file MD5.");
      }
      NiceName = ReadFixedString(reader, 64, 2);
      if (Version < 6)
      {
        HeaderMD5 = reader.ReadBytes(16);
        if (HeaderMD5 == null || HeaderMD5.Length != 16)
          throw new ApplicationException("Header length is not sufficient for header MD5.");
      }

      var nullable1 = Version switch
      {
          >= 9 => (long) reader.ReadUInt64(),
          >= 8 => reader.ReadUInt32(),
          _ => new long?()
      };
      var num1 = reader.ReadUInt32();
      var nullable2 = new long?();
      long dataOffset;
      if (Version >= 9)
      {
        dataOffset = (long) reader.ReadUInt64();
        nullable2 = (long) reader.ReadUInt64();
      }
      else
      {
        dataOffset = reader.ReadUInt32();
        if (Version >= 8)
          nullable2 = reader.ReadUInt32();
      }
      _ = (int) reader.ReadUInt32();
      if (Version >= 8)
        m_fileStream.Seek(256L, SeekOrigin.Current);
      var offset = nullable1 ?? m_fileStream.Position;
      if (offset + num1 > Length)
        throw new ApplicationException(string.Format("Header blob [{0} B, {1} B] outside of file length {1} B.", offset, offset + num1, Length));
      if (nullable2.HasValue && dataOffset + nullable2.Value > Length)
        throw new ApplicationException(string.Format("Data blob [{0} B, {1} B] outside of file length {1} B.", dataOffset, dataOffset + nullable2.Value, Length));
      if (m_fileStream.Seek(offset, SeekOrigin.Begin) != offset)
        throw new ApplicationException($"Unable to seek to position {offset} B.");
      var num3 = reader.ReadUInt32();
      var length1 = ReadIndex(reader);
      var num4 = reader.ReadUInt32();
      var length2 = ReadIndex(reader);
      var num5 = reader.ReadUInt32();
      var length3 = ReadIndex(reader);
      var num6 = reader.ReadUInt32();
      _ = (int) ReadIndex(reader);
      if (Version >= 7)
      {
        _ = (int) reader.ReadUInt32();
        if (Version >= 8)
        {
            _ = (int) reader.ReadUInt32();
        }
        BlockSize = reader.ReadUInt32();
      }
      if (m_fileStream.Seek(offset + num3, SeekOrigin.Begin) != offset + num3)
        throw new ApplicationException($"Unable to seek to position {offset + num3} B.");
      var tocDataArray = new TOCData[(int) length1];
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
      if (m_fileStream.Seek(offset + num4, SeekOrigin.Begin) != offset + num4)
        throw new ApplicationException($"Unable to seek to position {offset + num4} B.");
      var folderData = new FolderData[(int) length2];
      for (uint index = 0; index < length2; ++index)
      {
        folderData[(int) index].nameOffset = reader.ReadUInt32();
        folderData[(int) index].folderStartIndex = ReadIndex(reader);
        folderData[(int) index].folderEndIndex = ReadIndex(reader);
        folderData[(int) index].fileStartIndex = ReadIndex(reader);
        folderData[(int) index].fileEndIndex = ReadIndex(reader);
      }
      if (m_fileStream.Seek(offset + num5, SeekOrigin.Begin) != offset + num5)
        throw new ApplicationException($"Unable to seek to position {offset + num5} B.");
      var fileData = new FileData[(int) length3];
      for (uint index = 0; index < length3; ++index)
      {
        fileData[(int) index].nameOffset = reader.ReadUInt32();
        if (Version >= 8)
          fileData[(int) index].hashOffset = reader.ReadUInt32();
        fileData[(int) index].dataOffset = Version < 9 ? reader.ReadUInt32() : (long) reader.ReadUInt64();
        fileData[(int) index].length = reader.ReadUInt32();
        fileData[(int) index].storeLength = reader.ReadUInt32();
        if (Version < 10)
        {
            _ = (int) reader.ReadUInt32();
        }
        fileData[(int) index].verificationType = (FileVerificationType) reader.ReadByte();
        fileData[(int) index].storageType = (FileStorageType) reader.ReadByte();
        if (Version >= 6)
          fileData[(int) index].crc = reader.ReadUInt32();
        if (Version == 7)
          fileData[(int) index].hashOffset = reader.ReadUInt32();
      }
      var list = new List<INode>(tocDataArray.Length);
      foreach (var tocData in tocDataArray)
      {
        var children = CreateChildren(reader, folderData, tocData.folderRootIndex, tocData.folderRootIndex + 1U, fileData, 0U, 0U, offset + num6, dataOffset);
        var parent = new TOC(this, children, tocData.alias, tocData.name);
        SetParent(parent, children);
        list.Add(parent);
      }
      Children = new ReadOnlyCollection<INode>(list);
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
      var children1 = new List<INode>((int) folderEndIndex - (int) folderStartIndex + ((int) fileEndIndex - (int) fileStartIndex));
      for (var index = folderStartIndex; index < folderEndIndex; ++index)
      {
        var name = ReadDynamicString(reader, stringOffset + folderData[(int) index].nameOffset) ?? $"Folder_{index}";
        var num = name.LastIndexOfAny(new char[2]
        {
          Path.DirectorySeparatorChar,
          Path.AltDirectorySeparatorChar
        });
        if (num >= 0)
          name = name.Substring(num + 1);
        var children2 = CreateChildren(reader, folderData, folderData[(int) index].folderStartIndex, folderData[(int) index].folderEndIndex, fileData, folderData[(int) index].fileStartIndex, folderData[(int) index].fileEndIndex, stringOffset, dataOffset);
        if (name.Length > 0)
        {
          var parent = new Folder(this, children2, name);
          SetParent(parent, children2);
          children1.Add(parent);
        }
        else
          children1.AddRange(children2);
      }
      for (var index = fileStartIndex; index < fileEndIndex; ++index)
      {
        var fileOffset = dataOffset + fileData[(int) index].dataOffset;
        var name = ReadDynamicString(reader, stringOffset + fileData[(int) index].nameOffset);
        if (name == null && Version < 6)
        {
          var offset = fileOffset - 260L;
          name = m_fileStream.Seek(offset, SeekOrigin.Begin) != offset ? $"File_{index}.dat" : ReadFixedString(reader, 256, 1);
        }
        var crc32 = new uint?();
        if (Version >= 6)
          crc32 = fileData[(int) index].crc;
        children1.Add(new File(this, name, fileData[(int) index].storeLength, fileData[(int) index].length, fileData[(int) index].verificationType, fileData[(int) index].storageType, fileOffset, crc32));
      }
      return children1;
    }

    private static void SetParent(INode parent, IList<INode> children)
    {
      foreach (var child in children)
      {
        if (child is not Folder folder)
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
