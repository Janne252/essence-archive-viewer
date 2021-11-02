using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Essence.Core.IO
{
  public static class PathUtil
  {
    public static readonly char[] DirectorySeparatorChars = new char[2]
    {
      Path.DirectorySeparatorChar,
      Path.AltDirectorySeparatorChar
    };
    public const string UNCPathPrefix = "\\\\";
    public const string LongPathPrefix = "\\\\?\\";
    public const string UNCLongPathPrefix = "\\\\?\\UNC\\";
    private static readonly string s_currentDirectoryWithSuffix = "." + Path.DirectorySeparatorChar.ToString();

    public static string CanonicalizePath(string path) => string.Join(new string(Path.DirectorySeparatorChar, 1), CanonicalizePath(path, true));

    private static IEnumerable<string> CanonicalizePath(
      string path,
      bool preserveTrailingDirectorySeparatorChar)
    {
      var path1 = NormalizeDirectorySeparators(path);
      var pathRootLength = GetPathRootLength(path1);
      if (pathRootLength == 0)
      {
        path1 = Environment.CurrentDirectory + Path.DirectorySeparatorChar.ToString() + path;
        pathRootLength = GetPathRootLength(path1);
      }
      else if (pathRootLength == 1)
      {
        path1 = Path.GetPathRoot(Environment.CurrentDirectory) + path;
        pathRootLength = GetPathRootLength(path1);
      }
      if (pathRootLength == 0)
        throw new ApplicationException($"Unable to canonicalize path \"{path}\"");
      var strArray = path1.Substring(pathRootLength).Split(DirectorySeparatorChars);
      var source = new Stack<string>(strArray.Length);
      var flag = preserveTrailingDirectorySeparatorChar && strArray.Length != 0 && strArray[strArray.Length - 1] == "";
      var str1 = path1.Substring(0, pathRootLength);
      source.Push(str1);
      foreach (var str2 in strArray)
      {
        if (!(str2 == "") && !(str2 == "."))
        {
          if (str2 == "..")
          {
            source.Pop();
            if (source.Count == 0)
              source.Push(str1);
          }
          else
            source.Push(str2);
        }
      }
      if (flag)
        source.Push(string.Empty);
      return source.Reverse<string>();
    }

    public static string GetLongestCommonPath(string pathA, string pathB)
    {
      var values = PathsRelatable(pathA, pathB) ? CanonicalizePath(pathA, true).ToList<string>() : throw new InvalidOperationException("There is no common path for paths with different roots");
      var list = CanonicalizePath(pathB, true).ToList<string>();
      var stringStack = new Stack<string>(Math.Min(values.Count, list.Count));
      for (var index = 0; index < Math.Min(values.Count, list.Count) && PathComponentsEqual(values[index], list[index]); ++index)
        stringStack.Push(values[index]);
      return stringStack.Count == values.Count && stringStack.Count == list.Count ? string.Join(new string(Path.DirectorySeparatorChar, 1), values) : string.Join(new string(Path.DirectorySeparatorChar, 1), stringStack.ToArray());
    }

    public static bool IsPathWithin(string childPath, string parentPath)
    {
      if (!PathsRelatable(childPath, parentPath))
        return false;
      var list1 = CanonicalizePath(childPath, true).ToList<string>();
      var list2 = CanonicalizePath(parentPath, false).ToList<string>();
      if (list2.Count > list1.Count)
        return false;
      for (var index = 0; index < list2.Count; ++index)
      {
        if (!PathComponentsEqual(list1[index], list2[index]))
          return false;
      }
      return true;
    }

    public static string GetPathRelativeTo(string childPath, string parentPath)
    {
      var stringList1 = PathsRelatable(childPath, parentPath) ? CanonicalizePath(childPath, true).ToList<string>() : throw new InvalidOperationException("Paths with different roots cannot be made relative to each other");
      var list = CanonicalizePath(parentPath, false).ToList<string>();
      var index1 = 0;
      while (index1 < stringList1.Count && index1 < list.Count && PathComponentsEqual(stringList1[index1], list[index1]))
        ++index1;
      var stringList2 = new List<string>(list.Count - index1 + stringList1.Count - index1);
      for (var index2 = index1; index2 < list.Count; ++index2)
        stringList2.Add("..");
      for (var index3 = index1; index3 < stringList1.Count; ++index3)
        stringList2.Add(stringList1[index3]);
      return string.Join(new string(Path.DirectorySeparatorChar, 1), stringList2.ToArray());
    }

    public static string EnsureTrailingDirectorySeparatorChar(string path)
    {
      var length = path.Length;
      if (length <= 0)
        return s_currentDirectoryWithSuffix;
      return path[length - 1] != Path.DirectorySeparatorChar && path[length - 1] != Path.AltDirectorySeparatorChar ? path + Path.DirectorySeparatorChar.ToString() : path;
    }

    public static string NormalizeDirectorySeparators(string path)
    {
      var flag = true;
      for (var index = 0; index < path.Length; ++index)
      {
        var c = path[index];
        if (IsDirectorySeparator(c) && (c != Path.DirectorySeparatorChar || index > 0 && index + 1 < path.Length && IsDirectorySeparator(path[index + 1])))
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return path;
      var stringBuilder = new StringBuilder(path.Length);
      for (var index = 0; index < path.Length; ++index)
      {
        var directorySeparatorChar = path[index];
        if (IsDirectorySeparator(directorySeparatorChar))
        {
          if (index <= 0 || index + 1 >= path.Length || !IsDirectorySeparator(path[index + 1]))
            directorySeparatorChar = Path.DirectorySeparatorChar;
          else
            continue;
        }
        stringBuilder.Append(directorySeparatorChar);
      }
      return stringBuilder.ToString();
    }

    public static bool PathsRelatable(string pathA, string pathB) => PathComponentsEqual(Path.GetPathRoot(Path.IsPathRooted(pathA) ? pathA : Path.GetFullPath(".")), Path.GetPathRoot(Path.IsPathRooted(pathB) ? pathB : Path.GetFullPath(".")));

    public static bool PathComponentsEqual(string componentA, string componentB) => string.Equals(componentA, componentB, StringComparison.OrdinalIgnoreCase);

    public static bool IsDirectorySeparator(char c) => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;

    private static int GetPathRootLength(string path)
    {
      var length = path.Length;
      if (length == 0)
        return 0;
      var index = 0;
      var flag = false;
      if (path.StartsWith("\\\\?\\", StringComparison.Ordinal))
      {
        if (path.StartsWith("\\\\?\\UNC\\", StringComparison.Ordinal))
        {
          index = "\\\\?\\UNC\\".Length;
          flag = true;
        }
        else if (length >= "\\\\?\\".Length + 2 && path[5] == Path.VolumeSeparatorChar)
          index = "\\\\?\\".Length + 2;
      }
      else if (length >= 2 && path[1] == Path.VolumeSeparatorChar)
        index = 2;
      else if (path.StartsWith("\\\\", StringComparison.Ordinal))
      {
        index = "\\\\".Length;
        flag = true;
      }
      else if (path[0] == Path.DirectorySeparatorChar)
        index = 1;
      if (flag)
      {
        var num = 2;
        while (index < length && (path[index] != Path.DirectorySeparatorChar || --num > 0))
          ++index;
      }
      return index;
    }
  }
}
