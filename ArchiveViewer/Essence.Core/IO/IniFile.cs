// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.IniFile
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Essence.Core.IO
{
  public sealed class IniFile
  {
    private static readonly Regex CommentRegex = new Regex("^\\s*;", RegexOptions.Compiled);
    private static readonly Regex SectionRegex = new Regex("^\\s*\\[\\s*(.*)\\s*]\\s*$", RegexOptions.Compiled);
    private static readonly Regex KeyValueRegex = new Regex("^([^=]*)=(.*)$", RegexOptions.Compiled);

    public IniFile() => this.Sections = new Dictionary<string, Dictionary<string, string>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Dictionary<string, string>> Sections { get; }

    public bool TryGetValue(string sectionName, string key, out string value)
    {
      value = (string) null;
      Dictionary<string, string> dictionary;
      return this.Sections.TryGetValue(sectionName, out dictionary) && dictionary.TryGetValue(key, out value);
    }

    public string GetValue(string sectionName, string key) => this.Sections[sectionName][key];

    public void SetValue(string sectionName, string key, string value)
    {
      Dictionary<string, string> dictionary;
      if (!this.Sections.TryGetValue(sectionName, out dictionary))
      {
        dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        this.Sections.Add(sectionName, dictionary);
      }
      dictionary[key] = value;
    }

    public void Read(string fileName)
    {
      using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        this.Read((Stream) fileStream);
    }

    public void Read(Stream stream)
    {
      this.Sections.Clear();
      using (StreamReader streamReader = new StreamReader(stream))
      {
        Dictionary<string, string> dictionary = (Dictionary<string, string>) null;
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
          while (string.IsNullOrWhiteSpace(input) || IniFile.CommentRegex.IsMatch(input));
          Match match1 = IniFile.SectionRegex.Match(input);
          if (match1.Success)
          {
            key1 = match1.Groups[1].Value;
            if (!this.Sections.ContainsKey(key1))
            {
              dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
              this.Sections.Add(key1, dictionary);
            }
            else
              goto label_7;
          }
          else
          {
            Match match2 = IniFile.KeyValueRegex.Match(input);
            if (match2.Success)
            {
              key2 = match2.Groups[1].Value.Trim();
              string str = match2.Groups[2].Value.Trim();
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
        throw new ApplicationException(string.Format("Section '{0}' appears more than once in file.", (object) key1));
label_11:
        throw new ApplicationException(string.Format("Key '{0}' appears outside of section.", (object) key2));
label_13:
        throw new ApplicationException(string.Format("Key '{0}' appears more than once in section.", (object) key2));
label_15:
        throw new ApplicationException(string.Format("Error parsing '{0}'.", (object) input));
      }
    }

    public void Write(string fileName)
    {
      using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
        {
          foreach (KeyValuePair<string, Dictionary<string, string>> section in this.Sections)
          {
            streamWriter.WriteLine("[{0}]", (object) section.Key);
            foreach (KeyValuePair<string, string> keyValuePair in section.Value)
              streamWriter.WriteLine("{0}={1}", (object) section.Key, (object) keyValuePair.Value);
            streamWriter.WriteLine();
          }
        }
      }
    }
  }
}
