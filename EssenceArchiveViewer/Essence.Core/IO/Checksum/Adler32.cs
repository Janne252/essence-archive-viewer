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

    public Adler32() => HashSizeValue = 8 * Marshal.SizeOf(typeof (uint));

    public uint Adler32Hash => m_b << 16 | m_a;

    public override void Initialize()
    {
      m_a = 1U;
      m_b = 0U;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
      while (cbSize >= 5552)
      {
        var num = 694;
        do
        {
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
          m_a += array[ibStart++];
          m_b += m_a;
        }
        while (--num != 0);
        cbSize -= 5552;
        m_a %= 65521U;
        m_b %= 65521U;
      }
      if (cbSize <= 0)
        return;
      for (; cbSize >= 8; cbSize -= 8)
      {
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
        m_a += array[ibStart++];
        m_b += m_a;
      }
      for (; cbSize > 0; --cbSize)
      {
        m_a += array[ibStart++];
        m_b += m_a;
      }
      m_a %= 65521U;
      m_b %= 65521U;
    }

    protected override byte[] HashFinal() => BitConverter.GetBytes(Adler32Hash);
  }
}
