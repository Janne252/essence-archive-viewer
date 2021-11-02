using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class ChunkyWriter : IDisposable
  {
    private BinaryWriter m_binaryWriter;
    private readonly Stack<ChunkHeaderFixup> m_chunkHeaderFixups;

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
      m_binaryWriter = new BinaryWriter(stream, Chunky.Encoding, leaveOpen);
      m_chunkHeaderFixups = new Stack<ChunkHeaderFixup>();
      WriteHeader();
    }

    public void PushFolderChunk(FourCC id, uint version, string name) => PushChunk(Chunky.FolderType, id, version, name);

    public void PushDataChunk(FourCC id, uint version, string name) => PushChunk(Chunky.DataType, id, version, name);

    private void PushChunk(FourCC type, FourCC id, uint version, string name) => m_chunkHeaderFixups.Push(Write(new ChunkHeader(type, id, version, 0U, name)));

    public void PopChunk()
    {
      long position = m_binaryWriter.BaseStream.Position;
      ChunkHeaderFixup chunkHeaderFixup = m_chunkHeaderFixups.Pop();
      if (position > chunkHeaderFixup.DataStartPosition)
      {
        m_binaryWriter.BaseStream.Seek(chunkHeaderFixup.SizePosition, SeekOrigin.Begin);
        m_binaryWriter.Write((uint) (position - chunkHeaderFixup.DataStartPosition));
        m_binaryWriter.BaseStream.Seek(position, SeekOrigin.Begin);
      }
      else if (position < chunkHeaderFixup.DataStartPosition)
        throw new ApplicationException();
    }

    public void WriteDataChunk(FourCC id, uint version, string name, byte[] data)
    {
      Write(new ChunkHeader(Chunky.DataType, id, version, (uint) data.Length, name));
      m_binaryWriter.Write(data);
    }

    public void Write(bool value) => EnsureCanWriteData().Write(value);

    public void Write(sbyte value) => EnsureCanWriteData().Write(value);

    public void Write(byte value) => EnsureCanWriteData().Write(value);

    public void Write(short value) => EnsureCanWriteData().Write(value);

    public void Write(ushort value) => EnsureCanWriteData().Write(value);

    public void Write(int value) => EnsureCanWriteData().Write(value);

    public void Write(uint value) => EnsureCanWriteData().Write(value);

    public void Write(long value) => EnsureCanWriteData().Write(value);

    public void Write(ulong value) => EnsureCanWriteData().Write(value);

    public void Write(float value) => EnsureCanWriteData().Write(value);

    public void Write(double value) => EnsureCanWriteData().Write(value);

    public void Write(byte[] buffer) => EnsureCanWriteData().Write(buffer);

    public void Write(byte[] buffer, int index, int count) => EnsureCanWriteData().Write(buffer, index, count);

    public void Write(string value) => WriteString(EnsureCanWriteData(), value);

    private void WriteHeader()
    {
      m_binaryWriter.Write(Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0"));
      m_binaryWriter.Write(4U);
      m_binaryWriter.Write(1U);
    }

    private ChunkHeaderFixup Write(ChunkHeader chunkHeader)
    {
      if (m_binaryWriter == null)
        throw new InvalidOperationException();
      if (m_chunkHeaderFixups.Count > 0 && m_chunkHeaderFixups.Peek().Type != Chunky.FolderType)
        throw new ApplicationException(string.Format("Chunks can only be written to {0} chunks.", (object) Chunky.FolderType));
      WriteFourCC(m_binaryWriter, chunkHeader.Type);
      WriteFourCC(m_binaryWriter, chunkHeader.ID);
      m_binaryWriter.Write(chunkHeader.Version);
      long position = m_binaryWriter.BaseStream.Position;
      m_binaryWriter.Write(chunkHeader.Size);
      WriteString(m_binaryWriter, chunkHeader.Name);
      return new ChunkHeaderFixup(chunkHeader.Type, position, m_binaryWriter.BaseStream.Position);
    }

    public void Close() => Dispose(true);

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_binaryWriter == null)
        return;
      m_binaryWriter.Close();
      m_binaryWriter = (BinaryWriter) null;
    }

    private BinaryWriter EnsureCanWriteData()
    {
      if (m_binaryWriter == null)
        throw new InvalidOperationException();
      if (m_chunkHeaderFixups.Count == 0 || m_chunkHeaderFixups.Peek().Type != Chunky.DataType)
        throw new ApplicationException(string.Format("Data can only be written to {0} chunks.", (object) Chunky.DataType));
      return m_binaryWriter;
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
        Type = type;
        SizePosition = sizePosition;
        DataStartPosition = dataStartPosition;
      }

      public FourCC Type { get; }

      public long SizePosition { get; }

      public long DataStartPosition { get; }
    }
  }
}
