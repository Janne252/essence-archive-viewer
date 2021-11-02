using System;

namespace Essence.Core
{
  public struct FourCC : IEquatable<FourCC>
  {
    private uint m_value;

    public FourCC(uint value) => m_value = IsValid(value, out var _) ? value : throw new ArgumentOutOfRangeException(nameof (value));

    public uint Value
    {
      get => m_value;
      set => m_value = IsValid(value, out var _) ? value : throw new ArgumentOutOfRangeException(nameof (value));
    }

    public override bool Equals(object obj) => obj is FourCC other && Equals(other);

    public bool Equals(FourCC other) => (int) m_value == (int) other.m_value;

    public override int GetHashCode() => m_value.GetHashCode();

    public override string ToString()
    {
        if (!IsValid(m_value, out var length))
        return string.Empty;
      var num = 0;
      var chArray = new char[length];
      for (var index = 4 - length; index < 4; ++index)
      {
        var ch = (char) (m_value >> 8 * (3 - index) & byte.MaxValue);
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
            var index = 0;
            for (var length = value.Length; index < length; ++index)
            {
              var c = value[index];
              if (!IsValid(c))
                throw new ArgumentException($"Character [0x{c:X}] out of ASCII character range [0x20,0x7E].", nameof (value));
              if (num == 0U && c == ' ')
                throw new ArgumentException($"Spaces [0x{' ':X}] may not precede printing characters.", nameof (value));
              num = num << 8 | c;
            }
            return new FourCC(num);
          }
          goto case "";
      }
    }

    public static bool TryParse(string value, out FourCC result)
    {
      if (value is {Length: > 0 and <= 4})
      {
        uint num = 0;
        var index = 0;
        for (var length = value.Length; index < length; ++index)
        {
          var c = value[index];
          if (!IsValid(c) || num == 0U && c == ' ')
          {
            result = new FourCC();
            return false;
          }
          num = num << 8 | c;
        }
        result = new FourCC(num);
        return true;
      }
      result = new FourCC();
      return false;
    }

    private static bool IsValid(char c) => c is >= ' ' and <= '~';

    private static bool IsValid(uint fourCC, out int length)
    {
      length = 0;
      if (fourCC != 0U)
      {
        for (var index = 0; index < 4; ++index)
        {
          var c = (char) (fourCC >> 8 * (3 - index) & byte.MaxValue);
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
              if (!IsValid(c))
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
