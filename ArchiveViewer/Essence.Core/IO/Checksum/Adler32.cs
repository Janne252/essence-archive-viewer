// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Checksum.Adler32
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Essence.Core.IO.Checksum
{
  public class Adler32 : HashAlgorithm
  {
    public const int Base = 65521;
    public const int NMax = 5552;
    private uint m_a;
    private uint m_b;

    public Adler32() => this.HashSizeValue = 8 * Marshal.SizeOf(typeof (uint));

    public uint Adler32Hash => this.m_b << 16 | this.m_a;

    public override void Initialize()
    {
      this.m_a = 1U;
      this.m_b = 0U;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      while (cbSize >= 5552)
      {
        int num = 694;
        do
        {
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
          this.m_a += (uint) array[ibStart++];
          this.m_b += this.m_a;
        }
        while (--num != 0);
        cbSize -= 5552;
        this.m_a %= 65521U;
        this.m_b %= 65521U;
      }
      if (cbSize <= 0)
        return;
      for (; cbSize >= 8; cbSize -= 8)
      {
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
      }
      for (; cbSize > 0; --cbSize)
      {
        this.m_a += (uint) array[ibStart++];
        this.m_b += this.m_a;
      }
      this.m_a %= 65521U;
      this.m_b %= 65521U;
    }

    protected override byte[] HashFinal() => BitConverter.GetBytes(this.Adler32Hash);
  }
}
