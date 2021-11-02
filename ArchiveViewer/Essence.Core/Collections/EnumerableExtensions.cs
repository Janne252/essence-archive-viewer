// Decompiled with JetBrains decompiler
// Type: Essence.Core.Collections.EnumerableExtensions
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Collections.Generic;

namespace Essence.Core.Collections
{
  public static class EnumerableExtensions
  {
    public static void Deconstruct<TKey, TValue>(
      this KeyValuePair<TKey, TValue> keyValuePair,
      out TKey key,
      out TValue value)
    {
      key = keyValuePair.Key;
      value = keyValuePair.Value;
    }

    public static IEnumerable<TSource> Yield<TSource>(this TSource item)
    {
      yield return item;
    }

    public static TSource OneElementOrDefault<TSource>(this IEnumerable<TSource> source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (source is IList<TSource> sourceList)
        return sourceList.Count == 1 ? sourceList[0] : default (TSource);
      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          return default (TSource);
        TSource current = enumerator.Current;
        if (!enumerator.MoveNext())
          return current;
      }
      return default (TSource);
    }

    public static TSource OneElementOrDefault<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (predicate == null)
        throw new ArgumentNullException(nameof (predicate));
      TSource source1 = default (TSource);
      long num = 0;
      foreach (TSource source2 in source)
      {
        if (predicate(source2))
        {
          source1 = source2;
          checked { ++num; }
        }
      }
      if (num == 0L)
        return default (TSource);
      return num == 1L ? source1 : default (TSource);
    }

    public static int FirstIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (predicate == null)
        throw new ArgumentNullException(nameof (predicate));
      int num = 0;
      foreach (T obj in source)
      {
        if (predicate(obj))
          return num;
        ++num;
      }
      return -1;
    }
  }
}
