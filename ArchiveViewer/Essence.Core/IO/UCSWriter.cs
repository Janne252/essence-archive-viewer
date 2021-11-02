using System;
using System.Globalization;
using System.IO;

namespace Essence.Core.IO
{
  public sealed class UCSWriter : IDisposable
  {
    private TextWriter m_textWriter;
    private readonly bool m_escape;

    public UCSWriter(string fileName)
      : this(fileName, false)
    {
    }

    public UCSWriter(string fileName, bool escape)
      : this(new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), UCS.Encoding), escape)
    {
    }

    public UCSWriter(TextWriter textWriter)
      : this(textWriter, false)
    {
    }

    public UCSWriter(TextWriter textWriter, bool escape)
    {
      m_textWriter = textWriter ?? throw new ArgumentNullException(nameof (textWriter));
      m_escape = escape;
    }

    public void Dispose()
    {
      if (m_textWriter != null)
      {
        m_textWriter.Dispose();
        m_textWriter = null;
      }
      GC.SuppressFinalize(this);
    }

    public void Close() => Dispose();

    public void Write(int locStringID, string text)
    {
      m_textWriter.Write(locStringID.ToString(CultureInfo.InstalledUICulture));
      m_textWriter.Write('\t');
      if (!string.IsNullOrEmpty(text))
      {
        if (m_escape)
          m_textWriter.Write(UCS.Escape(text));
        else
          m_textWriter.Write(text);
      }
      m_textWriter.WriteLine();
    }
  }
}
