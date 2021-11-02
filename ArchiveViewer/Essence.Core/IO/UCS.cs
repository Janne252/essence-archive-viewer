using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Essence.Core.IO
{
  public static class UCS
  {
    public const string Extension = ".ucs";
    internal static Encoding Encoding = Encoding.Unicode;
    internal const char Delimiter = '\t';
    internal const char EscapeCharacter = '\\';
    internal static readonly ReadOnlyCollection<KeyValuePair<char, char>> EscapeSequences = new ReadOnlyCollection<KeyValuePair<char, char>>((IList<KeyValuePair<char, char>>) new KeyValuePair<char, char>[4]
    {
      new KeyValuePair<char, char>('\\', '\\'),
      new KeyValuePair<char, char>('n', '\n'),
      new KeyValuePair<char, char>('r', '\r'),
      new KeyValuePair<char, char>('t', '\t')
    });

    public static string Escape(string input)
    {
      int num = 0;
      foreach (char ch in input)
      {
        foreach (KeyValuePair<char, char> escapeSequence in EscapeSequences)
        {
          if ((int) ch == (int) escapeSequence.Value)
          {
            ++num;
            break;
          }
        }
      }
      if (num == 0)
        return input;
      StringBuilder stringBuilder = new StringBuilder(input.Length + num);
      foreach (char ch in input)
      {
        bool flag = false;
        foreach (KeyValuePair<char, char> escapeSequence in EscapeSequences)
        {
          if ((int) ch == (int) escapeSequence.Value)
          {
            stringBuilder.Append('\\');
            stringBuilder.Append(escapeSequence.Key);
            flag = true;
            break;
          }
        }
        if (!flag)
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    public static string Unescape(string input)
    {
      if (input.IndexOf('\\') == -1)
        return input;
      StringBuilder stringBuilder = new StringBuilder(input.Length);
      int index = 0;
      while (index < input.Length)
      {
        char ch = input[index];
        if (ch == '\\')
        {
          bool flag = false;
          if (index + 1 < input.Length)
          {
            foreach (KeyValuePair<char, char> escapeSequence in EscapeSequences)
            {
              if ((int) input[index + 1] == (int) escapeSequence.Key)
              {
                stringBuilder.Append(escapeSequence.Value);
                index += 2;
                flag = true;
                break;
              }
            }
          }
          if (!flag)
          {
            stringBuilder.Append(ch);
            ++index;
          }
        }
        else
        {
          stringBuilder.Append(ch);
          ++index;
        }
      }
      return stringBuilder.ToString();
    }
  }
}
