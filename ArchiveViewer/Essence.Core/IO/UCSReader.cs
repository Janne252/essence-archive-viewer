using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class UCSReader : IDisposable
  {
    private TextReader m_textReader;
    private readonly bool m_escape;

    public UCSReader(string fileName)
      : this(fileName, false)
    {
    }

    public UCSReader(string fileName, bool escape)
      : this(new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), UCS.Encoding), escape)
    {
    }

    public UCSReader(TextReader textReader)
      : this(textReader, false)
    {
    }

    public UCSReader(TextReader textReader, bool escape)
    {
      m_textReader = textReader ?? throw new ArgumentNullException(nameof (textReader));
      m_escape = escape;
    }

    public void Dispose()
    {
      if (m_textReader != null)
      {
        m_textReader.Dispose();
        m_textReader = null;
      }
      GC.SuppressFinalize(this);
    }

    public void Close() => Dispose();

    public bool Read(out int locStringID, out string text)
    {
      var str = m_textReader.ReadLine();
      if (str != null)
      {
        var length = str.IndexOf('\t');
        if (length != -1 && int.TryParse(str.Substring(0, length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
          var input = str.Substring(length + 1);
          if (m_escape)
            input = UCS.Unescape(input);
          locStringID = result;
          text = input;
          return true;
        }
      }
      locStringID = -1;
      text = null;
      return false;
    }

    public IEnumerable<KeyValuePair<int, string>> Read()
    {
      while (true)
      {
        int locStringID;
        string text;
        do
        {
        } while (!Read(out locStringID, out text));
        yield return new KeyValuePair<int, string>(locStringID, text);
      }
    }
  }
}
