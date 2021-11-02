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
    internal static readonly ReadOnlyCollection<KeyValuePair<char, char>> EscapeSequences = new(new KeyValuePair<char, char>[4]
    {
        new('\\', '\\'),
        new('n', '\n'),
        new('r', '\r'),
        new('t', '\t')
    });

    public static string Escape(string input)
    {
      var num = 0;
      foreach (var ch in input)
      {
        foreach (var escapeSequence in EscapeSequences)
        {
          if (ch == escapeSequence.Value)
          {
            ++num;
            break;
          }
        }
      }
      if (num == 0)
        return input;
      var stringBuilder = new StringBuilder(input.Length + num);
      foreach (var ch in input)
      {
        var flag = false;
        foreach (var escapeSequence in EscapeSequences)
        {
          if (ch == escapeSequence.Value)
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
      var stringBuilder = new StringBuilder(input.Length);
      var index = 0;
      while (index < input.Length)
      {
        var ch = input[index];
        if (ch == '\\')
        {
          var flag = false;
          if (index + 1 < input.Length)
          {
            foreach (var escapeSequence in EscapeSequences)
            {
              if (input[index + 1] == escapeSequence.Key)
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
