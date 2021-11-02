﻿// Decompiled with JetBrains decompiler
// Type: Essence.Core.FourCC
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core
{
  public struct FourCC : IEquatable<FourCC>
  {
    private uint m_value;

    public FourCC(uint value) => this.m_value = FourCC.IsValid(value, out int _) ? value : throw new ArgumentOutOfRangeException(nameof (value));

    public uint Value
    {
      get => this.m_value;
      set => this.m_value = FourCC.IsValid(value, out int _) ? value : throw new ArgumentOutOfRangeException(nameof (value));
    }

    public override bool Equals(object obj) => obj is FourCC other && this.Equals(other);

    public bool Equals(FourCC other) => (int) this.m_value == (int) other.m_value;

    public override int GetHashCode() => this.m_value.GetHashCode();

    public override string ToString()
    {
      int length;
      if (!FourCC.IsValid(this.m_value, out length))
        return string.Empty;
      int num = 0;
      char[] chArray = new char[length];
      for (int index = 4 - length; index < 4; ++index)
      {
        char ch = (char) (this.m_value >> 8 * (3 - index) & (uint) byte.MaxValue);
        chArray[num++] = ch;
      }
      return new string(chArray);
    }

    public static bool operator ==(FourCC lhs, FourCC rhs) => lhs.Equals(rhs);

    public static bool operator !=(FourCC lhs, FourCC rhs) => !lhs.Equals(rhs);

    public static FourCC Parse(string value)
    {
      switch (value)
      {
        case "":
          throw new ArgumentOutOfRangeException(nameof (value));
        case null:
          throw new ArgumentNullException(nameof (value));
        default:
          if (value.Length <= 4)
          {
            uint num = 0;
            int index = 0;
            for (int length = value.Length; index < length; ++index)
            {
              char c = value[index];
              if (!FourCC.IsValid(c))
                throw new ArgumentException(string.Format("Character [0x{0:X}] out of ASCII character range [0x20,0x7E].", (object) c), nameof (value));
              if (num == 0U && c == ' ')
                throw new ArgumentException(string.Format("Spaces [0x{0:X}] may not precede printing characters.", (object) ' '), nameof (value));
              num = num << 8 | (uint) c;
            }
            return new FourCC(num);
          }
          goto case "";
      }
    }

    public static bool TryParse(string value, out FourCC result)
    {
      if (value != null && value.Length > 0 && value.Length <= 4)
      {
        uint num = 0;
        int index = 0;
        for (int length = value.Length; index < length; ++index)
        {
          char c = value[index];
          if (!FourCC.IsValid(c) || num == 0U && c == ' ')
          {
            result = new FourCC();
            return false;
          }
          num = num << 8 | (uint) c;
        }
        result = new FourCC(num);
        return true;
      }
      result = new FourCC();
      return false;
    }

    private static bool IsValid(char c) => c >= ' ' && c <= '~';

    private static bool IsValid(uint fourCC, out int length)
    {
      length = 0;
      if (fourCC != 0U)
      {
        for (int index = 0; index < 4; ++index)
        {
          char c = (char) (fourCC >> 8 * (3 - index) & (uint) byte.MaxValue);
          switch (c)
          {
            case char.MinValue:
              if (length > 0)
              {
                length = 0;
                return false;
              }
              continue;
            case ' ':
              if (length == 0)
                return false;
              break;
            default:
              if (!FourCC.IsValid(c))
              {
                length = 0;
                return false;
              }
              break;
          }
          ++length;
        }
      }
      return true;
    }
  }
}