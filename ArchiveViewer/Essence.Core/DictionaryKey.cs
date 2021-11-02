// Decompiled with JetBrains decompiler
// Type: Essence.Core.DictionaryKey
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;

namespace Essence.Core
{
  public struct DictionaryKey : IComparable<DictionaryKey>, IEquatable<DictionaryKey>
  {
    public DictionaryKey(ulong hash)
    {
      this.Hash = hash;
      this.String = (string) null;
    }

    public DictionaryKey(string @string)
    {
      this.Hash = @string != null ? DictionaryHash.Hash(@string) : throw new ArgumentNullException(nameof (@string));
      this.String = @string;
    }

    internal DictionaryKey(ulong hash, string @string)
    {
      this.Hash = hash;
      this.String = @string;
    }

    public ulong Hash { get; }

    public string String { get; }

    public static bool operator ==(DictionaryKey lhs, DictionaryKey rhs) => lhs.Equals(rhs);

    public static bool operator !=(DictionaryKey lhs, DictionaryKey rhs) => !lhs.Equals(rhs);

    public int CompareTo(DictionaryKey other) => this.Hash.CompareTo(other.Hash);

    public bool Equals(DictionaryKey other) => (long) this.Hash == (long) other.Hash;

    public override bool Equals(object obj) => obj is DictionaryKey other && this.Equals(other);

    public override int GetHashCode() => this.Hash.GetHashCode();

    public override string ToString() => this.String ?? this.Hash.ToString("X8");
  }
}
