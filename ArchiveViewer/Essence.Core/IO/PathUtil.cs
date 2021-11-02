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
      string path1 = NormalizeDirectorySeparators(path);
      int pathRootLength = GetPathRootLength(path1);
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
        throw new ApplicationException(string.Format("Unable to canonicalize path \"{0}\"", (object) path));
      string[] strArray = path1.Substring(pathRootLength).Split(DirectorySeparatorChars);
      Stack<string> source = new Stack<string>(strArray.Length);
      bool flag = preserveTrailingDirectorySeparatorChar && strArray.Length != 0 && strArray[strArray.Length - 1] == "";
      string str1 = path1.Substring(0, pathRootLength);
      source.Push(str1);
      foreach (string str2 in strArray)
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
      List<string> values = PathsRelatable(pathA, pathB) ? CanonicalizePath(pathA, true).ToList<string>() : throw new InvalidOperationException("There is no common path for paths with different roots");
      List<string> list = CanonicalizePath(pathB, true).ToList<string>();
      Stack<string> stringStack = new Stack<string>(Math.Min(values.Count, list.Count));
      for (int index = 0; index < Math.Min(values.Count, list.Count) && PathComponentsEqual(values[index], list[index]); ++index)
        stringStack.Push(values[index]);
      return stringStack.Count == values.Count && stringStack.Count == list.Count ? string.Join(new string(Path.DirectorySeparatorChar, 1), (IEnumerable<string>) values) : string.Join(new string(Path.DirectorySeparatorChar, 1), stringStack.ToArray());
    }

    public static bool IsPathWithin(string childPath, string parentPath)
    {
      if (!PathsRelatable(childPath, parentPath))
        return false;
      List<string> list1 = CanonicalizePath(childPath, true).ToList<string>();
      List<string> list2 = CanonicalizePath(parentPath, false).ToList<string>();
      if (list2.Count > list1.Count)
        return false;
      for (int index = 0; index < list2.Count; ++index)
      {
        if (!PathComponentsEqual(list1[index], list2[index]))
          return false;
      }
      return true;
    }

    public static string GetPathRelativeTo(string childPath, string parentPath)
    {
      List<string> stringList1 = PathsRelatable(childPath, parentPath) ? CanonicalizePath(childPath, true).ToList<string>() : throw new InvalidOperationException("Paths with different roots cannot be made relative to each other");
      List<string> list = CanonicalizePath(parentPath, false).ToList<string>();
      int index1 = 0;
      while (index1 < stringList1.Count && index1 < list.Count && PathComponentsEqual(stringList1[index1], list[index1]))
        ++index1;
      List<string> stringList2 = new List<string>(list.Count - index1 + stringList1.Count - index1);
      for (int index2 = index1; index2 < list.Count; ++index2)
        stringList2.Add("..");
      for (int index3 = index1; index3 < stringList1.Count; ++index3)
        stringList2.Add(stringList1[index3]);
      return string.Join(new string(Path.DirectorySeparatorChar, 1), stringList2.ToArray());
    }

    public static string EnsureTrailingDirectorySeparatorChar(string path)
    {
      int length = path.Length;
      if (length <= 0)
        return s_currentDirectoryWithSuffix;
      return (int) path[length - 1] != (int) Path.DirectorySeparatorChar && (int) path[length - 1] != (int) Path.AltDirectorySeparatorChar ? path + Path.DirectorySeparatorChar.ToString() : path;
    }

    public static string NormalizeDirectorySeparators(string path)
    {
      bool flag = true;
      for (int index = 0; index < path.Length; ++index)
      {
        char c = path[index];
        if (IsDirectorySeparator(c) && ((int) c != (int) Path.DirectorySeparatorChar || index > 0 && index + 1 < path.Length && IsDirectorySeparator(path[index + 1])))
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return path;
      StringBuilder stringBuilder = new StringBuilder(path.Length);
      for (int index = 0; index < path.Length; ++index)
      {
        char directorySeparatorChar = path[index];
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

    public static bool IsDirectorySeparator(char c) => (int) c == (int) Path.DirectorySeparatorChar || (int) c == (int) Path.AltDirectorySeparatorChar;

    private static int GetPathRootLength(string path)
    {
      int length = path.Length;
      if (length == 0)
        return 0;
      int index = 0;
      bool flag = false;
      if (path.StartsWith("\\\\?\\", StringComparison.Ordinal))
      {
        if (path.StartsWith("\\\\?\\UNC\\", StringComparison.Ordinal))
        {
          index = "\\\\?\\UNC\\".Length;
          flag = true;
        }
        else if (length >= "\\\\?\\".Length + 2 && (int) path[5] == (int) Path.VolumeSeparatorChar)
          index = "\\\\?\\".Length + 2;
      }
      else if (length >= 2 && (int) path[1] == (int) Path.VolumeSeparatorChar)
        index = 2;
      else if (path.StartsWith("\\\\", StringComparison.Ordinal))
      {
        index = "\\\\".Length;
        flag = true;
      }
      else if ((int) path[0] == (int) Path.DirectorySeparatorChar)
        index = 1;
      if (flag)
      {
        int num = 2;
        while (index < length && ((int) path[index] != (int) Path.DirectorySeparatorChar || --num > 0))
          ++index;
      }
      return index;
    }
  }
}
