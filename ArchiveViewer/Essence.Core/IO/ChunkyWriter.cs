// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.ChunkyWriter
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class ChunkyWriter : IDisposable
  {
    private BinaryWriter m_binaryWriter;
    private readonly Stack<ChunkyWriter.ChunkHeaderFixup> m_chunkHeaderFixups;

    public ChunkyWriter(Stream stream)
      : this(stream, false)
    {
    }

    public ChunkyWriter(string fileName)
      : this((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
    {
    }

    public ChunkyWriter(Stream stream, bool leaveOpen)
    {
      this.m_binaryWriter = new BinaryWriter(stream, Chunky.Encoding, leaveOpen);
      this.m_chunkHeaderFixups = new Stack<ChunkyWriter.ChunkHeaderFixup>();
      this.WriteHeader();
    }

    public void PushFolderChunk(FourCC id, uint version, string name) => this.PushChunk(Chunky.FolderType, id, version, name);

    public void PushDataChunk(FourCC id, uint version, string name) => this.PushChunk(Chunky.DataType, id, version, name);

    private void PushChunk(FourCC type, FourCC id, uint version, string name) => this.m_chunkHeaderFixups.Push(this.Write(new ChunkHeader(type, id, version, 0U, name)));

    public void PopChunk()
    {
      long position = this.m_binaryWriter.BaseStream.Position;
      ChunkyWriter.ChunkHeaderFixup chunkHeaderFixup = this.m_chunkHeaderFixups.Pop();
      if (position > chunkHeaderFixup.DataStartPosition)
      {
        this.m_binaryWriter.BaseStream.Seek(chunkHeaderFixup.SizePosition, SeekOrigin.Begin);
        this.m_binaryWriter.Write((uint) (position - chunkHeaderFixup.DataStartPosition));
        this.m_binaryWriter.BaseStream.Seek(position, SeekOrigin.Begin);
      }
      else if (position < chunkHeaderFixup.DataStartPosition)
        throw new ApplicationException();
    }

    public void WriteDataChunk(FourCC id, uint version, string name, byte[] data)
    {
      this.Write(new ChunkHeader(Chunky.DataType, id, version, (uint) data.Length, name));
      this.m_binaryWriter.Write(data);
    }

    public void Write(bool value) => this.EnsureCanWriteData().Write(value);

    public void Write(sbyte value) => this.EnsureCanWriteData().Write(value);

    public void Write(byte value) => this.EnsureCanWriteData().Write(value);

    public void Write(short value) => this.EnsureCanWriteData().Write(value);

    public void Write(ushort value) => this.EnsureCanWriteData().Write(value);

    public void Write(int value) => this.EnsureCanWriteData().Write(value);

    public void Write(uint value) => this.EnsureCanWriteData().Write(value);

    public void Write(long value) => this.EnsureCanWriteData().Write(value);

    public void Write(ulong value) => this.EnsureCanWriteData().Write(value);

    public void Write(float value) => this.EnsureCanWriteData().Write(value);

    public void Write(double value) => this.EnsureCanWriteData().Write(value);

    public void Write(byte[] buffer) => this.EnsureCanWriteData().Write(buffer);

    public void Write(byte[] buffer, int index, int count) => this.EnsureCanWriteData().Write(buffer, index, count);

    public void Write(string value) => ChunkyWriter.WriteString(this.EnsureCanWriteData(), value);

    private void WriteHeader()
    {
      this.m_binaryWriter.Write(Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0"));
      this.m_binaryWriter.Write(4U);
      this.m_binaryWriter.Write(1U);
    }

    private ChunkyWriter.ChunkHeaderFixup Write(ChunkHeader chunkHeader)
    {
      if (this.m_binaryWriter == null)
        throw new InvalidOperationException();
      if (this.m_chunkHeaderFixups.Count > 0 && this.m_chunkHeaderFixups.Peek().Type != Chunky.FolderType)
        throw new ApplicationException(string.Format("Chunks can only be written to {0} chunks.", (object) Chunky.FolderType));
      ChunkyWriter.WriteFourCC(this.m_binaryWriter, chunkHeader.Type);
      ChunkyWriter.WriteFourCC(this.m_binaryWriter, chunkHeader.ID);
      this.m_binaryWriter.Write(chunkHeader.Version);
      long position = this.m_binaryWriter.BaseStream.Position;
      this.m_binaryWriter.Write(chunkHeader.Size);
      ChunkyWriter.WriteString(this.m_binaryWriter, chunkHeader.Name);
      return new ChunkyWriter.ChunkHeaderFixup(chunkHeader.Type, position, this.m_binaryWriter.BaseStream.Position);
    }

    public void Close() => this.Dispose(true);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || this.m_binaryWriter == null)
        return;
      this.m_binaryWriter.Close();
      this.m_binaryWriter = (BinaryWriter) null;
    }

    private BinaryWriter EnsureCanWriteData()
    {
      if (this.m_binaryWriter == null)
        throw new InvalidOperationException();
      if (this.m_chunkHeaderFixups.Count == 0 || this.m_chunkHeaderFixups.Peek().Type != Chunky.DataType)
        throw new ApplicationException(string.Format("Data can only be written to {0} chunks.", (object) Chunky.DataType));
      return this.m_binaryWriter;
    }

    private static void WriteFourCC(BinaryWriter binaryWriter, FourCC fourCC) => binaryWriter.Write((uint) (((int) fourCC.Value & (int) byte.MaxValue) << 24 | ((int) fourCC.Value & 65280) << 8) | (fourCC.Value & 16711680U) >> 8 | (fourCC.Value & 4278190080U) >> 24);

    private static void WriteString(BinaryWriter binaryWriter, string value)
    {
      if (!string.IsNullOrEmpty(value))
      {
        byte[] bytes = Chunky.Encoding.GetBytes(value);
        binaryWriter.Write((uint) bytes.Length);
        binaryWriter.Write(bytes);
      }
      else
        binaryWriter.Write(0U);
    }

    private struct ChunkHeaderFixup
    {
      public ChunkHeaderFixup(FourCC type, long sizePosition, long dataStartPosition)
      {
        this.Type = type;
        this.SizePosition = sizePosition;
        this.DataStartPosition = dataStartPosition;
      }

      public FourCC Type { get; }

      public long SizePosition { get; }

      public long DataStartPosition { get; }
    }
  }
}
