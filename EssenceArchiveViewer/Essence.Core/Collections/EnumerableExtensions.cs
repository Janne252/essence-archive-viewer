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
        return sourceList.Count == 1 ? sourceList[0] : default;
      using var enumerator = source.GetEnumerator();
      if (!enumerator.MoveNext())
          return default;
      var current = enumerator.Current;
      if (!enumerator.MoveNext())
          return current;
      return default;
    }

    public static TSource OneElementOrDefault<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (predicate == null)
        throw new ArgumentNullException(nameof (predicate));
      TSource source1 = default;
      long num = 0;
      foreach (var source2 in source)
      {
        if (predicate(source2))
        {
          source1 = source2;
          checked { ++num; }
        }
      }
      if (num == 0L)
        return default;
      return num == 1L ? source1 : default;
    }

    public static int FirstIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (predicate == null)
        throw new ArgumentNullException(nameof (predicate));
      var num = 0;
      foreach (var obj in source)
      {
        if (predicate(obj))
          return num;
        ++num;
      }
      return -1;
    }
  }
}
