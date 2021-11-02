using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Essence.Core.IO
{
  public sealed class IniFile
  {
    private static readonly Regex CommentRegex = new("^\\s*;", RegexOptions.Compiled);
    private static readonly Regex SectionRegex = new("^\\s*\\[\\s*(.*)\\s*]\\s*$", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new("^([^=]*)=(.*)$", RegexOptions.Compiled);

    public IniFile() => Sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Dictionary<string, string>> Sections { get; }

    public bool TryGetValue(string sectionName, string key, out string value)
    {
      value = null;
      return Sections.TryGetValue(sectionName, out var dictionary) && dictionary.TryGetValue(key, out value);
    }

    public string GetValue(string sectionName, string key) => Sections[sectionName][key];

    public void SetValue(string sectionName, string key, string value)
    {
        if (!Sections.TryGetValue(sectionName, out var dictionary))
      {
        dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Sections.Add(sectionName, dictionary);
      }
      dictionary[key] = value;
    }

    public void Read(string fileName)
    {
        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        Read(fileStream);
    }

    public void Read(Stream stream)
    {
      Sections.Clear();
      using var streamReader = new StreamReader(stream);
      var dictionary = (Dictionary<string, string>) null;
      string input;
      string key1;
      string key2;
      while (true)
      {
          do
          {
              input = streamReader.ReadLine();
              if (input == null)
                  goto label_3;
          }
          while (string.IsNullOrWhiteSpace(input) || CommentRegex.IsMatch(input));
          var match1 = SectionRegex.Match(input);
          if (match1.Success)
          {
              key1 = match1.Groups[1].Value;
              if (!Sections.ContainsKey(key1))
              {
                  dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                  Sections.Add(key1, dictionary);
              }
              else
                  goto label_7;
          }
          else
          {
              var match2 = KeyValueRegex.Match(input);
              if (match2.Success)
              {
                  key2 = match2.Groups[1].Value.Trim();
                  var str = match2.Groups[2].Value.Trim();
                  if (dictionary != null)
                  {
                      if (!dictionary.ContainsKey(key2))
                          dictionary.Add(key2, str);
                      else
                          goto label_13;
                  }
                  else
                      goto label_11;
              }
              else
                  goto label_15;
          }
      }
      label_3:
      return;
      label_7:
      throw new ApplicationException($"Section '{key1}' appears more than once in file.");
      label_11:
      throw new ApplicationException($"Key '{key2}' appears outside of section.");
      label_13:
      throw new ApplicationException($"Key '{key2}' appears more than once in section.");
      label_15:
      throw new ApplicationException($"Error parsing '{input}'.");
    }

    public void Write(string fileName)
    {
        using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var streamWriter = new StreamWriter(fileStream);
        foreach (var section in Sections)
        {
            streamWriter.WriteLine("[{0}]", section.Key);
            foreach (var keyValuePair in section.Value)
                streamWriter.WriteLine("{0}={1}", section.Key, keyValuePair.Value);
            streamWriter.WriteLine();
        }
    }
  }
}
