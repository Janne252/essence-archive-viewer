// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Wildcard
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Essence.Core.IO
{
  public static class Wildcard
  {
    private static readonly char[] WildcardCharacters = new char[2]
    {
      '*',
      '?'
    };

    public static Regex CreateWildcardRegex(string wildcard) => Wildcard.CreateWildcardRegex(wildcard, RegexOptions.None);

    public static Regex CreateWildcardRegex(string wildcard, RegexOptions regexOptions)
    {
      if (wildcard == null)
        throw new ArgumentNullException(nameof (wildcard));
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("^");
      int index;
      for (int startIndex = 0; startIndex < wildcard.Length; startIndex = index + 1)
      {
        index = wildcard.IndexOfAny(Wildcard.WildcardCharacters, startIndex);
        if (index == -1)
        {
          stringBuilder.Append(Regex.Escape(wildcard.Substring(startIndex)));
          break;
        }
        if (index > startIndex)
          stringBuilder.Append(Regex.Escape(wildcard.Substring(startIndex, index - startIndex)));
        switch (wildcard[index])
        {
          case '*':
            stringBuilder.Append(".*");
            break;
          case '?':
            stringBuilder.Append(".");
            break;
        }
      }
      stringBuilder.Append("$");
      return new Regex(stringBuilder.ToString(), regexOptions);
    }

    public static string[] GetFiles(string path, string wildcard)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (wildcard == null)
        throw new ArgumentNullException(nameof (wildcard));
      string[] pathParts = wildcard.Split(new char[2]
      {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
      }, StringSplitOptions.RemoveEmptyEntries);
      if (pathParts.Length == 0)
        return new string[0];
      List<string> files = new List<string>();
      Wildcard.GetFiles(path, pathParts, 0, SearchOption.TopDirectoryOnly, files);
      return files.ToArray();
    }

    private static void GetFiles(
      string path,
      string[] pathParts,
      int index,
      SearchOption searchOption,
      List<string> files)
    {
      if (Wildcard.IsWildcard(pathParts[index]))
      {
        if (index + 1 < pathParts.Length)
        {
          if (pathParts[index] == "**")
          {
            if (index + 2 != pathParts.Length)
              throw new ApplicationException("Recursive wildcard pattern only allowed on last directory.");
            Wildcard.GetFiles(path, pathParts, ++index, SearchOption.AllDirectories, files);
          }
          else
          {
            try
            {
              foreach (string directory in Directory.GetDirectories(path, pathParts[index], searchOption))
                Wildcard.GetFiles(directory, pathParts, ++index, searchOption, files);
            }
            catch (DirectoryNotFoundException ex)
            {
            }
          }
        }
        else
        {
          try
          {
            foreach (string file in Directory.GetFiles(path, pathParts[index], searchOption))
              files.Add(file);
          }
          catch (DirectoryNotFoundException ex)
          {
          }
        }
      }
      else if (index + 1 < pathParts.Length)
        Wildcard.GetFiles(Path.Combine(path, pathParts[index]), pathParts, ++index, searchOption, files);
      else
        files.Add(Path.Combine(path, pathParts[index]));
    }

    public static bool IsWildcard(string @string)
    {
      if (@string == null)
        throw new ArgumentNullException(nameof (@string));
      return @string.IndexOfAny(Wildcard.WildcardCharacters) != -1;
    }
  }
}
