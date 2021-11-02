using System;
using System.IO;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class TarWriter : IDisposable
  {
      private Stream m_stream;
    private readonly byte[] m_block;

    public TarWriter(Stream stream)
    {
      m_stream = stream ?? throw new ArgumentNullException();
      m_block = new byte[512];
    }

    public TarWriter(string fileName)
      : this(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
    {
    }

    public void AppendFolder(string name)
    {
      if (m_stream == null)
        throw new InvalidOperationException();
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException();
      name = name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      if (name[name.Length - 1] != Path.AltDirectorySeparatorChar)
        name += Path.AltDirectorySeparatorChar.ToString();
      if (name.Length > 99)
      {
        AppendLongLink(name, 16895U, 0U);
        name = name.Substring(0, 99);
      }
      AppendHeader(name, 16895U, 0U, 0U, '5');
    }

    public void AppendFile(string name, Stream source)
    {
      if (m_stream == null)
        throw new InvalidOperationException();
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException();
      name = name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      var mtime = source is FileStream fileStream ? FileTime.GetLastWriteTime(fileStream.Name) : 0U;
      if (name.Length > 99)
      {
        AppendLongLink(name, 33279U, mtime);
        name = name.Substring(0, 99);
      }
      AppendHeader(name, 33279U, (uint) source.Length, mtime, '0');
      AppendStream(source);
    }

    public void Close() => Dispose(true);

    private void AppendLongLink(string name, uint mode, uint mtime)
    {
      var numArray = new byte[Encoding.ASCII.GetByteCount(name) + 1];
      Encoding.ASCII.GetBytes(name, 0, name.Length, numArray, 0);
      AppendHeader("././@LongLink", mode, (uint) numArray.Length, mtime, 'L');
      AppendStream(new MemoryStream(numArray));
    }

    private void AppendHeader(string name, uint mode, uint size, uint mtime, char type)
    {
      Array.Clear(m_block, 0, m_block.Length);
      var offset1 = 0;
      AppendHeaderField(name, 100, true, ref offset1);
      AppendHeaderField(mode, 8, true, ref offset1);
      AppendHeaderField(0U, 8, true, ref offset1);
      AppendHeaderField(0U, 8, true, ref offset1);
      AppendHeaderField(size, 12, false, ref offset1);
      AppendHeaderField(mtime, 12, false, ref offset1);
      var offset2 = offset1;
      AppendHeaderField(new string(' ', 8), 8, false, ref offset1);
      AppendHeaderField(new string(type, 1), 1, false, ref offset1);
      AppendHeaderField(string.Empty, 100, true, ref offset1);
      uint num1 = 0;
      foreach (var num2 in m_block)
        num1 += num2;
      AppendHeaderField(num1, 8, true, ref offset2);
      m_stream.Write(m_block, 0, m_block.Length);
    }

    private void AppendHeaderField(string value, int capacity, bool nullTerminate, ref int offset)
    {
      if (Encoding.ASCII.GetBytes(value, 0, value.Length, m_block, offset) + (nullTerminate ? 1 : 0) > capacity)
        throw new ApplicationException($"String {value} too long for field.");
      offset += capacity;
    }

    private void AppendHeaderField(uint value, int capacity, bool nullTerminate, ref int offset)
    {
      var s = Convert.ToString(value, 8).PadLeft(capacity - (nullTerminate ? 2 : 1), ' ').PadRight(capacity - (nullTerminate ? 1 : 0), ' ');
      if (Encoding.ASCII.GetBytes(s, 0, s.Length, m_block, offset) + (nullTerminate ? 1 : 0) > capacity)
        throw new ApplicationException($"Number {value} too long for field.");
      offset += capacity;
    }

    private void AppendStream(Stream source)
    {
      long length;
      for (length = source.Length; length >= m_block.Length; length -= m_block.Length)
      {
        source.Read(m_block, 0, m_block.Length);
        m_stream.Write(m_block, 0, m_block.Length);
      }
      if (length <= 0L)
        return;
      source.Read(m_block, 0, (int) length);
      Array.Clear(m_block, (int) length, m_block.Length - (int) length);
      m_stream.Write(m_block, 0, m_block.Length);
    }

    private void AppendEndOfFile()
    {
      Array.Clear(m_block, 0, m_block.Length);
      m_stream.Write(m_block, 0, m_block.Length);
      m_stream.Write(m_block, 0, m_block.Length);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || m_stream == null)
        return;
      AppendEndOfFile();
      m_stream.Close();
      m_stream = null;
    }
  }
}
