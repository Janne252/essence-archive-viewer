// Decompiled with JetBrains decompiler
// Type: Essence.Core.DictionaryHash
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Globalization;
using System.Text;

namespace Essence.Core
{
  public static class DictionaryHash
  {
    private static ulong k0 = 14097894508562428199;
    private static ulong k1 = 13011662864482103923;
    private static ulong k2 = 11160318154034397263;

    public static ulong Hash(string @string) => DictionaryHash.CityHash64(Encoding.ASCII.GetBytes(@string.ToLower(CultureInfo.InvariantCulture)));

    private static ulong ByteSwap(ulong value) => (ulong) ((long) ((value & 18374686479671623680UL) >> 56) | (long) ((value & 71776119061217280UL) >> 40) | (long) ((value & 280375465082880UL) >> 24) | (long) ((value & 1095216660480UL) >> 8) | ((long) value & 4278190080L) << 8 | ((long) value & 16711680L) << 24 | ((long) value & 65280L) << 40 | ((long) value & (long) byte.MaxValue) << 56);

    private static ulong Rotate(ulong val, int shift) => shift != 0 ? val >> shift | val << 64 - shift : val;

    private static ulong Hash128to64(DictionaryHash.UInt128 x)
    {
      ulong num1 = (ulong) (((long) x.Low ^ (long) x.High) * -7070675565921424023L);
      ulong num2 = num1 ^ num1 >> 47;
      long num3 = ((long) x.High ^ (long) num2) * -7070675565921424023L;
      return (ulong) ((num3 ^ (long) ((ulong) num3 >> 47)) * -7070675565921424023L);
    }

    private static ulong ShiftMix(ulong val) => val ^ val >> 47;

    private static ulong HashLen16(ulong u, ulong v) => DictionaryHash.Hash128to64(new DictionaryHash.UInt128(u, v));

    private static ulong HashLen16(ulong u, ulong v, ulong mul)
    {
      ulong num1 = (u ^ v) * mul;
      ulong num2 = num1 ^ num1 >> 47;
      long num3 = ((long) v ^ (long) num2) * (long) mul;
      return ((ulong) num3 ^ (ulong) num3 >> 47) * mul;
    }

    private static ulong HashLen0to16(byte[] s, int len)
    {
      if (len >= 8)
      {
        ulong mul = DictionaryHash.k2 + (ulong) len * 2UL;
        ulong val = BitConverter.ToUInt64(s, 0) + DictionaryHash.k2;
        ulong uint64 = BitConverter.ToUInt64(s, len - 8);
        return DictionaryHash.HashLen16(DictionaryHash.Rotate(uint64, 37) * mul + val, (DictionaryHash.Rotate(val, 25) + uint64) * mul, mul);
      }
      if (len >= 4)
      {
        ulong mul = DictionaryHash.k2 + (ulong) len * 2UL;
        ulong uint32 = (ulong) BitConverter.ToUInt32(s, 0);
        return DictionaryHash.HashLen16((ulong) len + (uint32 << 3), (ulong) BitConverter.ToUInt32(s, len - 4), mul);
      }
      if (len <= 0)
        return DictionaryHash.k2;
      int num1 = (int) s[0];
      byte num2 = s[len >> 1];
      byte num3 = s[len - 1];
      int num4 = (int) num2 << 8;
      int num5 = num1 + num4;
      uint num6 = (uint) (len + ((int) num3 << 2));
      return DictionaryHash.ShiftMix((ulong) ((long) (uint) num5 * (long) DictionaryHash.k2 ^ (long) num6 * (long) DictionaryHash.k0)) * DictionaryHash.k2;
    }

    private static ulong HashLen17to32(byte[] s, int len)
    {
      ulong mul = DictionaryHash.k2 + (ulong) len * 2UL;
      ulong num1 = BitConverter.ToUInt64(s, 0) * DictionaryHash.k1;
      ulong uint64 = BitConverter.ToUInt64(s, 8);
      ulong val = BitConverter.ToUInt64(s, len - 8) * mul;
      ulong num2 = BitConverter.ToUInt64(s, len - 16) * DictionaryHash.k2;
      return DictionaryHash.HashLen16(DictionaryHash.Rotate(num1 + uint64, 43) + DictionaryHash.Rotate(val, 30) + num2, num1 + DictionaryHash.Rotate(uint64 + DictionaryHash.k2, 18) + val, mul);
    }

    private static DictionaryHash.UInt128 WeakHashLen32WithSeeds(
      ulong w,
      ulong x,
      ulong y,
      ulong z,
      ulong a,
      ulong b)
    {
      a += w;
      b = DictionaryHash.Rotate(b + a + z, 21);
      ulong num = a;
      a += x;
      a += y;
      b += DictionaryHash.Rotate(a, 44);
      DictionaryHash.UInt128 uint128;
      uint128.Low = a + z;
      uint128.High = b + num;
      return uint128;
    }

    private static DictionaryHash.UInt128 WeakHashLen32WithSeeds(
      byte[] s,
      int offset,
      ulong a,
      ulong b)
    {
      return DictionaryHash.WeakHashLen32WithSeeds(BitConverter.ToUInt64(s, offset), BitConverter.ToUInt64(s, offset + 8), BitConverter.ToUInt64(s, offset + 16), BitConverter.ToUInt64(s, offset + 24), a, b);
    }

    private static ulong HashLen33to64(byte[] s, int len)
    {
      ulong num1 = DictionaryHash.k2 + (ulong) len * 2UL;
      ulong num2 = BitConverter.ToUInt64(s, 0) * DictionaryHash.k2;
      ulong uint64_1 = BitConverter.ToUInt64(s, 8);
      ulong uint64_2 = BitConverter.ToUInt64(s, len - 24);
      ulong uint64_3 = BitConverter.ToUInt64(s, len - 32);
      long num3 = (long) BitConverter.ToUInt64(s, 16) * (long) DictionaryHash.k2;
      ulong num4 = BitConverter.ToUInt64(s, 24) * 9UL;
      ulong uint64_4 = BitConverter.ToUInt64(s, len - 8);
      ulong num5 = BitConverter.ToUInt64(s, len - 16) * num1;
      long num6 = (long) DictionaryHash.Rotate(num2 + uint64_4, 43) + ((long) DictionaryHash.Rotate(uint64_1, 30) + (long) uint64_2) * 9L;
      ulong num7 = (num2 + uint64_4 ^ uint64_3) + num4 + 1UL;
      long num8 = (long) num7;
      ulong num9 = DictionaryHash.ByteSwap((ulong) (num6 + num8) * num1) + num5;
      ulong num10 = DictionaryHash.Rotate((ulong) num3 + num4, 42) + uint64_2;
      ulong num11 = (DictionaryHash.ByteSwap((num7 + num9) * num1) + uint64_4) * num1;
      ulong num12 = (ulong) num3 + num4 + uint64_2;
      ulong num13 = DictionaryHash.ByteSwap((num10 + num12) * num1 + num11) + uint64_1;
      return DictionaryHash.ShiftMix((num12 + num13) * num1 + uint64_3 + num5) * num1 + num10;
    }

    public static ulong CityHash64(byte[] s)
    {
      int length = s.Length;
      if (length <= 32)
        return length <= 16 ? DictionaryHash.HashLen0to16(s, length) : DictionaryHash.HashLen17to32(s, length);
      if (length <= 64)
        return DictionaryHash.HashLen33to64(s, length);
      ulong uint64 = BitConverter.ToUInt64(s, length - 40);
      ulong val = BitConverter.ToUInt64(s, length - 16) + BitConverter.ToUInt64(s, length - 56);
      ulong b = DictionaryHash.HashLen16(BitConverter.ToUInt64(s, length - 48) + (ulong) length, BitConverter.ToUInt64(s, length - 24));
      DictionaryHash.UInt128 uint128_1 = DictionaryHash.WeakHashLen32WithSeeds(s, length - 64, (ulong) length, b);
      DictionaryHash.UInt128 uint128_2 = DictionaryHash.WeakHashLen32WithSeeds(s, length - 32, val + DictionaryHash.k1, uint64);
      ulong num1 = uint64 * DictionaryHash.k1 + BitConverter.ToUInt64(s, 0);
      int num2 = length - 1 & -64;
      int offset = 0;
      do
      {
        ulong num3 = DictionaryHash.Rotate(num1 + val + uint128_1.Low + BitConverter.ToUInt64(s, offset + 8), 37) * DictionaryHash.k1;
        ulong num4 = DictionaryHash.Rotate(val + uint128_1.High + BitConverter.ToUInt64(s, offset + 48), 42) * DictionaryHash.k1;
        ulong num5 = num3 ^ uint128_2.High;
        val = num4 + (uint128_1.Low + BitConverter.ToUInt64(s, offset + 40));
        ulong num6 = DictionaryHash.Rotate(b + uint128_2.Low, 33) * DictionaryHash.k1;
        uint128_1 = DictionaryHash.WeakHashLen32WithSeeds(s, offset, uint128_1.High * DictionaryHash.k1, num5 + uint128_2.Low);
        uint128_2 = DictionaryHash.WeakHashLen32WithSeeds(s, offset + 32, num6 + uint128_2.High, val + BitConverter.ToUInt64(s, offset + 16));
        long num7 = (long) num5;
        num1 = num6;
        b = (ulong) num7;
        offset += 64;
        num2 -= 64;
      }
      while (num2 != 0);
      return DictionaryHash.HashLen16(DictionaryHash.HashLen16(uint128_1.Low, uint128_2.Low) + DictionaryHash.ShiftMix(val) * DictionaryHash.k1 + b, DictionaryHash.HashLen16(uint128_1.High, uint128_2.High) + num1);
    }

    private struct UInt128
    {
      public ulong Low;
      public ulong High;

      public UInt128(ulong low, ulong high)
      {
        this.Low = low;
        this.High = high;
      }
    }
  }
}
