﻿// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.TarWriter
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.IO;
using System.Text;

namespace Essence.Core.IO
{
  public sealed class TarWriter : IDisposable
  {
    private const int BlockSize = 512;
    private const char TypeFile = '0';
    private const char TypeFolder = '5';
    private const char TypeLink = 'L';
    private const int NameSize = 100;
    private const string LongLink = "././@LongLink";
    private Stream m_stream;
    private byte[] m_block;

    public TarWriter(Stream stream)
    {
      this.m_stream = stream ?? throw new ArgumentNullException();
      this.m_block = new byte[512];
    }

    public TarWriter(string fileName)
      : this((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
    {
    }

    public void AppendFolder(string name)
    {
      if (this.m_stream == null)
        throw new InvalidOperationException();
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException();
      name = name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      if ((int) name[name.Length - 1] != (int) Path.AltDirectorySeparatorChar)
        name += Path.AltDirectorySeparatorChar.ToString();
      if (name.Length > 99)
      {
        this.AppendLongLink(name, 16895U, 0U);
        name = name.Substring(0, 99);
      }
      this.AppendHeader(name, 16895U, 0U, 0U, '5');
    }

    public void AppendFile(string name, Stream source)
    {
      if (this.m_stream == null)
        throw new InvalidOperationException();
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException();
      name = name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      uint mtime = source is FileStream fileStream ? FileTime.GetLastWriteTime(fileStream.Name) : 0U;
      if (name.Length > 99)
      {
        this.AppendLongLink(name, 33279U, mtime);
        name = name.Substring(0, 99);
      }
      this.AppendHeader(name, 33279U, (uint) source.Length, mtime, '0');
      this.AppendStream(source);
    }

    public void Close() => this.Dispose(true);

    private void AppendLongLink(string name, uint mode, uint mtime)
    {
      byte[] numArray = new byte[Encoding.ASCII.GetByteCount(name) + 1];
      Encoding.ASCII.GetBytes(name, 0, name.Length, numArray, 0);
      this.AppendHeader("././@LongLink", mode, (uint) numArray.Length, mtime, 'L');
      this.AppendStream((Stream) new MemoryStream(numArray));
    }

    private void AppendHeader(string name, uint mode, uint size, uint mtime, char type)
    {
      Array.Clear((Array) this.m_block, 0, this.m_block.Length);
      int offset1 = 0;
      this.AppendHeaderField(name, 100, true, ref offset1);
      this.AppendHeaderField(mode, 8, true, ref offset1);
      this.AppendHeaderField(0U, 8, true, ref offset1);
      this.AppendHeaderField(0U, 8, true, ref offset1);
      this.AppendHeaderField(size, 12, false, ref offset1);
      this.AppendHeaderField(mtime, 12, false, ref offset1);
      int offset2 = offset1;
      this.AppendHeaderField(new string(' ', 8), 8, false, ref offset1);
      this.AppendHeaderField(new string(type, 1), 1, false, ref offset1);
      this.AppendHeaderField(string.Empty, 100, true, ref offset1);
      uint num1 = 0;
      foreach (byte num2 in this.m_block)
        num1 += (uint) num2;
      this.AppendHeaderField(num1, 8, true, ref offset2);
      this.m_stream.Write(this.m_block, 0, this.m_block.Length);
    }

    private void AppendHeaderField(string value, int capacity, bool nullTerminate, ref int offset)
    {
      if (Encoding.ASCII.GetBytes(value, 0, value.Length, this.m_block, offset) + (nullTerminate ? 1 : 0) > capacity)
        throw new ApplicationException(string.Format("String {0} too long for field.", (object) value));
      offset += capacity;
    }

    private void AppendHeaderField(uint value, int capacity, bool nullTerminate, ref int offset)
    {
      string s = Convert.ToString((long) value, 8).PadLeft(capacity - (nullTerminate ? 2 : 1), ' ').PadRight(capacity - (nullTerminate ? 1 : 0), ' ');
      if (Encoding.ASCII.GetBytes(s, 0, s.Length, this.m_block, offset) + (nullTerminate ? 1 : 0) > capacity)
        throw new ApplicationException(string.Format("Number {0} too long for field.", (object) value));
      offset += capacity;
    }

    private void AppendStream(Stream source)
    {
      long length;
      for (length = source.Length; length >= (long) this.m_block.Length; length -= (long) this.m_block.Length)
      {
        source.Read(this.m_block, 0, this.m_block.Length);
        this.m_stream.Write(this.m_block, 0, this.m_block.Length);
      }
      if (length <= 0L)
        return;
      source.Read(this.m_block, 0, (int) length);
      Array.Clear((Array) this.m_block, (int) length, this.m_block.Length - (int) length);
      this.m_stream.Write(this.m_block, 0, this.m_block.Length);
    }

    private void AppendEndOfFile()
    {
      Array.Clear((Array) this.m_block, 0, this.m_block.Length);
      this.m_stream.Write(this.m_block, 0, this.m_block.Length);
      this.m_stream.Write(this.m_block, 0, this.m_block.Length);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing || this.m_stream == null)
        return;
      this.AppendEndOfFile();
      this.m_stream.Close();
      this.m_stream = (Stream) null;
    }
  }
}
