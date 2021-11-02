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
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, data, (object[]) null))
        return;
      string empty = string.Empty;
      if (data != null)
        empty = data.ToString();
      writeLine(empty, eventType);
    }

    public override void TraceData(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      params object[] data)
    {
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, (string) null, (object[]) null, (object) null, data))
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
      writeLine(stringBuilder.ToString(), eventType);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id)
    {
      TraceEvent(eventCache, source, eventType, id, string.Empty);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      string message)
    {
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, (object[]) null, (object) null, (object[]) null))
        return;
      writeLine(message, eventType);
    }

    public override void TraceEvent(
      TraceEventCache eventCache,
      string source,
      TraceEventType eventType,
      int id,
      string format,
      params object[] args)
    {
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, (object) null, (object[]) null))
        return;
      if (args != null)
        writeLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, args), eventType);
      else
        writeLine(format, eventType);
    }

    public override void Write(string message) => Write(message, TraceEventType.Information);

    public override void WriteLine(string message) => writeLine(message, TraceEventType.Information);

    protected abstract void Write(string message, TraceEventType eventType);

    public virtual void writeLine(string message, TraceEventType eventType) => Write(message + Environment.NewLine, eventType);
  }
}
