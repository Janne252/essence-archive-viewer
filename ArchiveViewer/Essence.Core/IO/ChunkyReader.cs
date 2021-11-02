using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class ChunkyReader : IDisposable
  {
    private BinaryReader m_binaryReader;
    private readonly Stack<ChunkHeaderPosition> m_chunkHeaderPositions;
    private ChunkHeaderPosition? m_nextChunkHeaderPosition;

    public ChunkyReader(Stream stream)
      : this(stream, false)
    {
    }

    public ChunkyReader(string fileName)
      : this((Stream) new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    public ChunkyReader(Stream stream, bool leaveOpen)
    {
      m_binaryReader = new BinaryReader(stream, Chunky.Encoding, leaveOpen);
      m_chunkHeaderPositions = new Stack<ChunkHeaderPosition>();
      ReadHeader();
    }

    public ChunkHeader? PeakChunk()
    {
      PeakNextChunk(PeakBehaviour.PreservePosition);
      ref ChunkHeaderPosition? local = ref m_nextChunkHeaderPosition;
      return !local.HasValue ? new ChunkHeader?() : new ChunkHeader?(local.GetValueOrDefault().Header);
    }

    public ChunkHeader PushFolderChunk(FourCC id, uint version) => PushChunk(Chunky.FolderType, id, version);

    public ChunkHeader PushDataChunk(FourCC id, uint version) => PushChunk(Chunky.DataType, id, version);

    public ChunkHeader PushChunk(FourCC type, FourCC id, uint version)
    {
      PeakNextChunk(PeakBehaviour.PositionAtChunkData);
      if (!m_nextChunkHeaderPosition.HasValue)
        throw new EndOfStreamException();
      if (m_nextChunkHeaderPosition.Value.Header.Type != type || m_nextChunkHeaderPosition.Value.Header.ID != id)
        throw new IOException(string.Format("{0} chunk [{1}] not found.", (object) type, (object) id));
      if (m_nextChunkHeaderPosition.Value.Header.Version > version)
        throw new IOException(string.Format("{0} chunk [{1}] version [{2}] newer than expected version [{3}].", (object) type, (object) id, (object) m_nextChunkHeaderPosition.Value.Header.Version, (object) version));
      m_chunkHeaderPositions.Push(m_nextChunkHeaderPosition.Value);
      m_nextChunkHeaderPosition = new ChunkHeaderPosition?();
      return m_chunkHeaderPositions.Peek().Header;
    }

    public void PopChunk()
    {
      ChunkHeaderPosition chunkHeaderPosition = m_chunkHeaderPositions.Pop();
      m_nextChunkHeaderPosition = new ChunkHeaderPosition?();
      m_binaryReader.BaseStream.Seek(chunkHeaderPosition.DataPosition + (long) chunkHeaderPosition.Header.Size, SeekOrigin.Begin);
    }

    public bool SkipNextChunk()
    {
      PeakNextChunk(PeakBehaviour.PositionAtChunkData);
      if (!m_nextChunkHeaderPosition.HasValue)
        return false;
      long size = (long) m_nextChunkHeaderPosition.Value.Header.Size;
      m_nextChunkHeaderPosition = new ChunkHeaderPosition?();
      m_binaryReader.BaseStream.Seek(size, SeekOrigin.Current);
      return true;
    }

    public bool ReadBoolean() => EnsureCanReadData().ReadBoolean();

    public sbyte ReadSByte() => EnsureCanReadData().ReadSByte();

    public byte ReadByte() => EnsureCanReadData().ReadByte();

    public short ReadInt16() => EnsureCanReadData().ReadInt16();

    public ushort ReadUInt16() => EnsureCanReadData().ReadUInt16();

    public int ReadInt32() => EnsureCanReadData().ReadInt32();

    public uint ReadUInt32() => EnsureCanReadData().ReadUInt32();

    public long ReadInt64() => EnsureCanReadData().ReadInt64();

    public ulong ReadUInt64() => EnsureCanReadData().ReadUInt64();

    public float ReadSingle() => EnsureCanReadData().ReadSingle();

    public double ReadDouble() => EnsureCanReadData().ReadDouble();

    public byte[] ReadBytes(int count) => EnsureCanReadData().ReadBytes(count);

    public void Read(byte[] buffer, int index, int count) => EnsureCanReadData().Read(buffer, index, count);

    public string ReadString() => ReadString(EnsureCanReadData());

    private void ReadHeader()
    {
      byte[] bytes = Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0");
      byte[] second = m_binaryReader.ReadBytes(bytes.Length);
      if (!((IEnumerable<byte>) bytes).SequenceEqual<byte>((IEnumerable<byte>) second))
        throw new IOException("Not a chunky file.");
      uint num1 = m_binaryReader.ReadUInt32();
      if (num1 != 4U)
        throw new IOException(string.Format("Unsupported chunky version [{0}].", (object) num1));
      uint num2 = m_binaryReader.ReadUInt32();
      if (num2 != 1U)
        throw new IOException(string.Format("Unsupported chunky platform [{0}].", (object) num2));
    }

    private void PeakNextChunk(PeakBehaviour peakBehaviour)
    {
      if (m_binaryReader == null)
        throw new InvalidOperationException();
      ChunkHeaderPosition chunkHeaderPosition;
      if (m_chunkHeaderPositions.Count > 0)
      {
        chunkHeaderPosition = m_chunkHeaderPositions.Peek();
        if (chunkHeaderPosition.Header.Type != Chunky.FolderType)
          throw new ApplicationException(string.Format("Chunks can only be read from {0} chunks.", (object) Chunky.FolderType));
      }
      if (m_nextChunkHeaderPosition.HasValue)
      {
        if (peakBehaviour != PeakBehaviour.PositionAtChunkData)
          return;
        Stream baseStream = m_binaryReader.BaseStream;
        chunkHeaderPosition = m_nextChunkHeaderPosition.Value;
        long dataPosition = chunkHeaderPosition.DataPosition;
        baseStream.Seek(dataPosition, SeekOrigin.Begin);
      }
      else
      {
        long position1 = m_binaryReader.BaseStream.Position;
        if (m_chunkHeaderPositions.Count > 0)
        {
          long num1 = position1;
          chunkHeaderPosition = m_chunkHeaderPositions.Peek();
          long dataPosition = chunkHeaderPosition.DataPosition;
          chunkHeaderPosition = m_chunkHeaderPositions.Peek();
          long size = (long) chunkHeaderPosition.Header.Size;
          long num2 = dataPosition + size;
          if (num1 == num2)
            return;
        }
        FourCC type;
        try
        {
          type = ReadFourCC(m_binaryReader);
        }
        catch (EndOfStreamException ex)
        {
          return;
        }
        if (type != Chunky.DataType && type != Chunky.FolderType)
          throw new IOException(string.Format("Unsupported chunk type [{0}].", (object) type));
        FourCC id = ReadFourCC(m_binaryReader);
        uint version = m_binaryReader.ReadUInt32();
        uint size1 = m_binaryReader.ReadUInt32();
        string name = ReadString(m_binaryReader);
        long position2 = m_binaryReader.BaseStream.Position;
        m_nextChunkHeaderPosition = new ChunkHeaderPosition?(new ChunkHeaderPosition(new ChunkHeader(type, id, version, size1, name), position1, position2));
        if (peakBehaviour != PeakBehaviour.PreservePosition)
          return;
        m_binaryReader.BaseStream.Seek(position1, SeekOrigin.Begin);
      }
    }

    public void Close() => Dispose(true);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_binaryReader == null)
        return;
      m_binaryReader.Close();
      m_binaryReader = (BinaryReader) null;
    }

    private BinaryReader EnsureCanReadData()
    {
      if (m_binaryReader == null)
        throw new InvalidOperationException();
      if (m_chunkHeaderPositions.Count == 0 || m_chunkHeaderPositions.Peek().Header.Type != Chunky.DataType)
        throw new ApplicationException(string.Format("Data can only be read from {0} chunks.", (object) Chunky.DataType));
      return m_binaryReader;
    }

    private static FourCC ReadFourCC(BinaryReader binaryReader)
    {
      uint num = binaryReader.ReadUInt32();
      return new FourCC((uint) (((int) num & (int) byte.MaxValue) << 24 | ((int) num & 65280) << 8) | (num & 16711680U) >> 8 | (num & 4278190080U) >> 24);
    }

    private static string ReadString(BinaryReader binaryReader)
    {
      uint count = binaryReader.ReadUInt32();
      return count > 0U ? Chunky.Encoding.GetString(binaryReader.ReadBytes((int) count)) : string.Empty;
    }

    private enum PeakBehaviour
    {
      PreservePosition,
      PositionAtChunkData,
    }

    private struct ChunkHeaderPosition
    {
      public ChunkHeaderPosition(ChunkHeader header, long chunkPosition, long dataPosition)
      {
        Header = header;
        ChunkPosition = chunkPosition;
        DataPosition = dataPosition;
      }

      public ChunkHeader Header { get; }

      public long ChunkPosition { get; }

      public long DataPosition { get; }
    }
  }
}
