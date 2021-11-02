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
      : this((TextReader) new StreamReader((Stream) new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), UCS.Encoding), escape)
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
        m_textReader = (TextReader) null;
      }
      GC.SuppressFinalize((object) this);
    }

    public void Close() => Dispose();

    public bool Read(out int locStringID, out string text)
    {
      string str = m_textReader.ReadLine();
      if (str != null)
      {
        int length = str.IndexOf('\t');
        int result;
        if (length != -1 && int.TryParse(str.Substring(0, length), NumberStyles.Integer, (IFormatProvider) CultureInfo.InvariantCulture, out result))
        {
          string input = str.Substring(length + 1);
          if (m_escape)
            input = UCS.Unescape(input);
          locStringID = result;
          text = input;
          return true;
        }
      }
      locStringID = -1;
      text = (string) null;
      return false;
    }

    public IEnumerable<KeyValuePair<int, string>> Read()
    {
      while (true)
      {
        int locStringID;
        string text;
        do
          ;
        while (!Read(out locStringID, out text));
        yield return new KeyValuePair<int, string>(locStringID, text);
      }
    }
  }
}
