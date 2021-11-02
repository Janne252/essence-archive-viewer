// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.ChunkyReader
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
    private readonly Stack<ChunkyReader.ChunkHeaderPosition> m_chunkHeaderPositions;
    private ChunkyReader.ChunkHeaderPosition? m_nextChunkHeaderPosition;

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
      this.m_binaryReader = new BinaryReader(stream, Chunky.Encoding, leaveOpen);
      this.m_chunkHeaderPositions = new Stack<ChunkyReader.ChunkHeaderPosition>();
      this.ReadHeader();
    }

    public ChunkHeader? PeakChunk()
    {
      this.PeakNextChunk(ChunkyReader.PeakBehaviour.PreservePosition);
      ref ChunkyReader.ChunkHeaderPosition? local = ref this.m_nextChunkHeaderPosition;
      return !local.HasValue ? new ChunkHeader?() : new ChunkHeader?(local.GetValueOrDefault().Header);
    }

    public ChunkHeader PushFolderChunk(FourCC id, uint version) => this.PushChunk(Chunky.FolderType, id, version);

    public ChunkHeader PushDataChunk(FourCC id, uint version) => this.PushChunk(Chunky.DataType, id, version);

    public ChunkHeader PushChunk(FourCC type, FourCC id, uint version)
    {
      this.PeakNextChunk(ChunkyReader.PeakBehaviour.PositionAtChunkData);
      if (!this.m_nextChunkHeaderPosition.HasValue)
        throw new EndOfStreamException();
      if (this.m_nextChunkHeaderPosition.Value.Header.Type != type || this.m_nextChunkHeaderPosition.Value.Header.ID != id)
        throw new IOException(string.Format("{0} chunk [{1}] not found.", (object) type, (object) id));
      if (this.m_nextChunkHeaderPosition.Value.Header.Version > version)
        throw new IOException(string.Format("{0} chunk [{1}] version [{2}] newer than expected version [{3}].", (object) type, (object) id, (object) this.m_nextChunkHeaderPosition.Value.Header.Version, (object) version));
      this.m_chunkHeaderPositions.Push(this.m_nextChunkHeaderPosition.Value);
      this.m_nextChunkHeaderPosition = new ChunkyReader.ChunkHeaderPosition?();
      return this.m_chunkHeaderPositions.Peek().Header;
    }

    public void PopChunk()
    {
      ChunkyReader.ChunkHeaderPosition chunkHeaderPosition = this.m_chunkHeaderPositions.Pop();
      this.m_nextChunkHeaderPosition = new ChunkyReader.ChunkHeaderPosition?();
      this.m_binaryReader.BaseStream.Seek(chunkHeaderPosition.DataPosition + (long) chunkHeaderPosition.Header.Size, SeekOrigin.Begin);
    }

    public bool SkipNextChunk()
    {
      this.PeakNextChunk(ChunkyReader.PeakBehaviour.PositionAtChunkData);
      if (!this.m_nextChunkHeaderPosition.HasValue)
        return false;
      long size = (long) this.m_nextChunkHeaderPosition.Value.Header.Size;
      this.m_nextChunkHeaderPosition = new ChunkyReader.ChunkHeaderPosition?();
      this.m_binaryReader.BaseStream.Seek(size, SeekOrigin.Current);
      return true;
    }

    public bool ReadBoolean() => this.EnsureCanReadData().ReadBoolean();

    public sbyte ReadSByte() => this.EnsureCanReadData().ReadSByte();

    public byte ReadByte() => this.EnsureCanReadData().ReadByte();

    public short ReadInt16() => this.EnsureCanReadData().ReadInt16();

    public ushort ReadUInt16() => this.EnsureCanReadData().ReadUInt16();

    public int ReadInt32() => this.EnsureCanReadData().ReadInt32();

    public uint ReadUInt32() => this.EnsureCanReadData().ReadUInt32();

    public long ReadInt64() => this.EnsureCanReadData().ReadInt64();

    public ulong ReadUInt64() => this.EnsureCanReadData().ReadUInt64();

    public float ReadSingle() => this.EnsureCanReadData().ReadSingle();

    public double ReadDouble() => this.EnsureCanReadData().ReadDouble();

    public byte[] ReadBytes(int count) => this.EnsureCanReadData().ReadBytes(count);

    public void Read(byte[] buffer, int index, int count) => this.EnsureCanReadData().Read(buffer, index, count);

    public string ReadString() => ChunkyReader.ReadString(this.EnsureCanReadData());

    private void ReadHeader()
    {
      byte[] bytes = Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0");
      byte[] second = this.m_binaryReader.ReadBytes(bytes.Length);
      if (!((IEnumerable<byte>) bytes).SequenceEqual<byte>((IEnumerable<byte>) second))
        throw new IOException("Not a chunky file.");
      uint num1 = this.m_binaryReader.ReadUInt32();
      if (num1 != 4U)
        throw new IOException(string.Format("Unsupported chunky version [{0}].", (object) num1));
      uint num2 = this.m_binaryReader.ReadUInt32();
      if (num2 != 1U)
        throw new IOException(string.Format("Unsupported chunky platform [{0}].", (object) num2));
    }

    private void PeakNextChunk(ChunkyReader.PeakBehaviour peakBehaviour)
    {
      if (this.m_binaryReader == null)
        throw new InvalidOperationException();
      ChunkyReader.ChunkHeaderPosition chunkHeaderPosition;
      if (this.m_chunkHeaderPositions.Count > 0)
      {
        chunkHeaderPosition = this.m_chunkHeaderPositions.Peek();
        if (chunkHeaderPosition.Header.Type != Chunky.FolderType)
          throw new ApplicationException(string.Format("Chunks can only be read from {0} chunks.", (object) Chunky.FolderType));
      }
      if (this.m_nextChunkHeaderPosition.HasValue)
      {
        if (peakBehaviour != ChunkyReader.PeakBehaviour.PositionAtChunkData)
          return;
        Stream baseStream = this.m_binaryReader.BaseStream;
        chunkHeaderPosition = this.m_nextChunkHeaderPosition.Value;
        long dataPosition = chunkHeaderPosition.DataPosition;
        baseStream.Seek(dataPosition, SeekOrigin.Begin);
      }
      else
      {
        long position1 = this.m_binaryReader.BaseStream.Position;
        if (this.m_chunkHeaderPositions.Count > 0)
        {
          long num1 = position1;
          chunkHeaderPosition = this.m_chunkHeaderPositions.Peek();
          long dataPosition = chunkHeaderPosition.DataPosition;
          chunkHeaderPosition = this.m_chunkHeaderPositions.Peek();
          long size = (long) chunkHeaderPosition.Header.Size;
          long num2 = dataPosition + size;
          if (num1 == num2)
            return;
        }
        FourCC type;
        try
        {
          type = ChunkyReader.ReadFourCC(this.m_binaryReader);
        }
        catch (EndOfStreamException ex)
        {
          return;
        }
        if (type != Chunky.DataType && type != Chunky.FolderType)
          throw new IOException(string.Format("Unsupported chunk type [{0}].", (object) type));
        FourCC id = ChunkyReader.ReadFourCC(this.m_binaryReader);
        uint version = this.m_binaryReader.ReadUInt32();
        uint size1 = this.m_binaryReader.ReadUInt32();
        string name = ChunkyReader.ReadString(this.m_binaryReader);
        long position2 = this.m_binaryReader.BaseStream.Position;
        this.m_nextChunkHeaderPosition = new ChunkyReader.ChunkHeaderPosition?(new ChunkyReader.ChunkHeaderPosition(new ChunkHeader(type, id, version, size1, name), position1, position2));
        if (peakBehaviour != ChunkyReader.PeakBehaviour.PreservePosition)
          return;
        this.m_binaryReader.BaseStream.Seek(position1, SeekOrigin.Begin);
      }
    }

    public void Close() => this.Dispose(true);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || this.m_binaryReader == null)
        return;
      this.m_binaryReader.Close();
      this.m_binaryReader = (BinaryReader) null;
    }

    private BinaryReader EnsureCanReadData()
    {
      if (this.m_binaryReader == null)
        throw new InvalidOperationException();
      if (this.m_chunkHeaderPositions.Count == 0 || this.m_chunkHeaderPositions.Peek().Header.Type != Chunky.DataType)
        throw new ApplicationException(string.Format("Data can only be read from {0} chunks.", (object) Chunky.DataType));
      return this.m_binaryReader;
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
        this.Header = header;
        this.ChunkPosition = chunkPosition;
        this.DataPosition = dataPosition;
      }

      public ChunkHeader Header { get; }

      public long ChunkPosition { get; }

      public long DataPosition { get; }
    }
  }
}
