// Decompiled with JetBrains decompiler
// Type: Essence.Core.Diagnostics.FriendlyTraceListener
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Essence.Core.Diagnostics
{
  public abstract class FriendlyTraceListener : TraceListener
  {
    public override void TraceData(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      object data)
    {
      if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, data, (object[]) null))
        return;
      string empty = string.Empty;
      if (data != null)
        empty = data.ToString();
      this.writeLine(empty, eventType);
    }

    public override void TraceData(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      params object[] data)
    {
      if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, (object) null, data))
        return;
      StringBuilder stringBuilder = new StringBuilder();
      if (data != null)
      {
        for (int index = 0; index < data.Length; ++index)
        {
          if (index != 0)
            stringBuilder.Append(", ");
          if (data[index] != null)
            stringBuilder.Append(data[index].ToString());
        }
      }
      this.writeLine(stringBuilder.ToString(), eventType);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id)
    {
      this.TraceEvent(eventCache, source, eventType, id, string.Empty);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      string message)
    {
      if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, message, (object[]) null, (object) null, (object[]) null))
        return;
      this.writeLine(message, eventType);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      string format,
      params object[] args)
    {
      if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, (object) null, (object[]) null))
        return;
      if (args != null)
        this.writeLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, args), eventType);
      else
        this.writeLine(format, eventType);
    }

    public override void Write(string message) => this.Write(message, TraceEventType.Information);

    public override void WriteLine(string message) => this.writeLine(message, TraceEventType.Information);

    protected abstract void Write(string message, TraceEventType eventType);

    public virtual void writeLine(string message, TraceEventType eventType) => this.Write(message + Environment.NewLine, eventType);
  }
}
