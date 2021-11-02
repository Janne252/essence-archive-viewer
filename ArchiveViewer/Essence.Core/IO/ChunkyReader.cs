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
      : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
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
      ref var local = ref m_nextChunkHeaderPosition;
      return !local.HasValue ? new ChunkHeader?() : local.GetValueOrDefault().Header;
    }

    public ChunkHeader PushFolderChunk(FourCC id, uint version) => PushChunk(Chunky.FolderType, id, version);

    public ChunkHeader PushDataChunk(FourCC id, uint version) => PushChunk(Chunky.DataType, id, version);

    public ChunkHeader PushChunk(FourCC type, FourCC id, uint version)
    {
      PeakNextChunk(PeakBehaviour.PositionAtChunkData);
      if (!m_nextChunkHeaderPosition.HasValue)
        throw new EndOfStreamException();
      if (m_nextChunkHeaderPosition.Value.Header.Type != type || m_nextChunkHeaderPosition.Value.Header.ID != id)
        throw new IOException($"{type} chunk [{id}] not found.");
      if (m_nextChunkHeaderPosition.Value.Header.Version > version)
        throw new IOException(
            $"{type} chunk [{id}] version [{m_nextChunkHeaderPosition.Value.Header.Version}] newer than expected version [{version}].");
      m_chunkHeaderPositions.Push(m_nextChunkHeaderPosition.Value);
      m_nextChunkHeaderPosition = new ChunkHeaderPosition?();
      return m_chunkHeaderPositions.Peek().Header;
    }

    public void PopChunk()
    {
      var chunkHeaderPosition = m_chunkHeaderPositions.Pop();
      m_nextChunkHeaderPosition = new ChunkHeaderPosition?();
      m_binaryReader.BaseStream.Seek(chunkHeaderPosition.DataPosition + chunkHeaderPosition.Header.Size, SeekOrigin.Begin);
    }

    public bool SkipNextChunk()
    {
      PeakNextChunk(PeakBehaviour.PositionAtChunkData);
      if (!m_nextChunkHeaderPosition.HasValue)
        return false;
      var size = (long) m_nextChunkHeaderPosition.Value.Header.Size;
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
      var bytes = Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0");
      var second = m_binaryReader.ReadBytes(bytes.Length);
      if (!bytes.SequenceEqual<byte>(second))
        throw new IOException("Not a chunky file.");
      var num1 = m_binaryReader.ReadUInt32();
      if (num1 != 4U)
        throw new IOException($"Unsupported chunky version [{num1}].");
      var num2 = m_binaryReader.ReadUInt32();
      if (num2 != 1U)
        throw new IOException($"Unsupported chunky platform [{num2}].");
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
          throw new ApplicationException($"Chunks can only be read from {Chunky.FolderType} chunks.");
      }
      if (m_nextChunkHeaderPosition.HasValue)
      {
        if (peakBehaviour != PeakBehaviour.PositionAtChunkData)
          return;
        var baseStream = m_binaryReader.BaseStream;
        chunkHeaderPosition = m_nextChunkHeaderPosition.Value;
        var dataPosition = chunkHeaderPosition.DataPosition;
        baseStream.Seek(dataPosition, SeekOrigin.Begin);
      }
      else
      {
        var position1 = m_binaryReader.BaseStream.Position;
        if (m_chunkHeaderPositions.Count > 0)
        {
          var num1 = position1;
          chunkHeaderPosition = m_chunkHeaderPositions.Peek();
          var dataPosition = chunkHeaderPosition.DataPosition;
          chunkHeaderPosition = m_chunkHeaderPositions.Peek();
          var size = (long) chunkHeaderPosition.Header.Size;
          var num2 = dataPosition + size;
          if (num1 == num2)
            return;
        }
        FourCC type;
        try
        {
          type = ReadFourCC(m_binaryReader);
        }
        catch (EndOfStreamException)
        {
          return;
        }
        if (type != Chunky.DataType && type != Chunky.FolderType)
          throw new IOException($"Unsupported chunk type [{type}].");
        var id = ReadFourCC(m_binaryReader);
        var version = m_binaryReader.ReadUInt32();
        var size1 = m_binaryReader.ReadUInt32();
        var name = ReadString(m_binaryReader);
        var position2 = m_binaryReader.BaseStream.Position;
        m_nextChunkHeaderPosition = new ChunkHeaderPosition(new ChunkHeader(type, id, version, size1, name), position1, position2);
        if (peakBehaviour != PeakBehaviour.PreservePosition)
          return;
        m_binaryReader.BaseStream.Seek(position1, SeekOrigin.Begin);
      }
    }

    public void Close() => Dispose(true);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_binaryReader == null)
        return;
      m_binaryReader.Close();
      m_binaryReader = null;
    }

    private BinaryReader EnsureCanReadData()
    {
      if (m_binaryReader == null)
        throw new InvalidOperationException();
      if (m_chunkHeaderPositions.Count == 0 || m_chunkHeaderPositions.Peek().Header.Type != Chunky.DataType)
        throw new ApplicationException($"Data can only be read from {Chunky.DataType} chunks.");
      return m_binaryReader;
    }

    private static FourCC ReadFourCC(BinaryReader binaryReader)
    {
      var num = binaryReader.ReadUInt32();
      return new FourCC((uint) (((int) num & byte.MaxValue) << 24 | ((int) num & 65280) << 8) | (num & 16711680U) >> 8 | (num & 4278190080U) >> 24);
    }

    private static string ReadString(BinaryReader binaryReader)
    {
      var count = binaryReader.ReadUInt32();
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
