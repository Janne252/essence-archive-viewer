using System;

namespace Essence.Core
{
  public struct DictionaryKey : IComparable<DictionaryKey>, IEquatable<DictionaryKey>
  {
    public DictionaryKey(ulong hash)
    {
      Hash = hash;
      String = null;
    }

    public DictionaryKey(string @string)
    {
      Hash = @string != null ? DictionaryHash.Hash(@string) : throw new ArgumentNullException(nameof (@string));
      String = @string;
    }

    internal DictionaryKey(ulong hash, string @string)
    {
      Hash = hash;
      String = @string;
    }

    public ulong Hash { get; }

    public string String { get; }

    public static bool operator ==(DictionaryKey lhs, DictionaryKey rhs) => lhs.Equals(rhs);

    public static bool operator !=(DictionaryKey lhs, DictionaryKey rhs) => !lhs.Equals(rhs);

    public int CompareTo(DictionaryKey other) => Hash.CompareTo(other.Hash);

    public bool Equals(DictionaryKey other) => (long) Hash == (long) other.Hash;

    public override bool Equals(object obj) => obj is DictionaryKey other && Equals(other);

    public override int GetHashCode() => Hash.GetHashCode();

    public override string ToString() => String ?? Hash.ToString("X8");
  }
}
