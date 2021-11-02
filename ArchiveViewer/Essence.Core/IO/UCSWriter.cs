// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.UCSWriter
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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
      : this((TextWriter) new StreamWriter((Stream) new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), UCS.Encoding), escape)
    {
    }

    public UCSWriter(TextWriter textWriter)
      : this(textWriter, false)
    {
    }

    public UCSWriter(TextWriter textWriter, bool escape)
    {
      this.m_textWriter = textWriter ?? throw new ArgumentNullException(nameof (textWriter));
      this.m_escape = escape;
    }

    public void Dispose()
    {
      if (this.m_textWriter != null)
      {
        this.m_textWriter.Dispose();
        this.m_textWriter = (TextWriter) null;
      }
      GC.SuppressFinalize((object) this);
    }

    public void Close() => this.Dispose();

    public void Write(int locStringID, string text)
    {
      this.m_textWriter.Write(locStringID.ToString((IFormatProvider) CultureInfo.InstalledUICulture));
      this.m_textWriter.Write('\t');
      if (!string.IsNullOrEmpty(text))
      {
        if (this.m_escape)
          this.m_textWriter.Write(UCS.Escape(text));
        else
          this.m_textWriter.Write(text);
      }
      this.m_textWriter.WriteLine();
    }
  }
}
