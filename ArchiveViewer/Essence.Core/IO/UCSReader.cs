// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.UCSReader
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      this.m_textReader = textReader ?? throw new ArgumentNullException(nameof (textReader));
      this.m_escape = escape;
    }

    public void Dispose()
    {
      if (this.m_textReader != null)
      {
        this.m_textReader.Dispose();
        this.m_textReader = (TextReader) null;
      }
      GC.SuppressFinalize((object) this);
    }

    public void Close() => this.Dispose();

    public bool Read(out int locStringID, out string text)
    {
      string str = this.m_textReader.ReadLine();
      if (str != null)
      {
        int length = str.IndexOf('\t');
        int result;
        if (length != -1 && int.TryParse(str.Substring(0, length), NumberStyles.Integer, (IFormatProvider) CultureInfo.InvariantCulture, out result))
        {
          string input = str.Substring(length + 1);
          if (this.m_escape)
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
        while (!this.Read(out locStringID, out text));
        yield return new KeyValuePair<int, string>(locStringID, text);
      }
    }
  }
}
