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
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
        return;
      var empty = string.Empty;
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
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
        return;
      var stringBuilder = new StringBuilder();
      if (data != null)
      {
        for (var index = 0; index < data.Length; ++index)
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
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
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
      if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
        return;
      if (args != null)
        writeLine(string.Format(CultureInfo.InvariantCulture, format, args), eventType);
      else
        writeLine(format, eventType);
    }

    public override void Write(string message) => Write(message, TraceEventType.Information);

    public override void WriteLine(string message) => writeLine(message, TraceEventType.Information);

    protected abstract void Write(string message, TraceEventType eventType);

#pragma warning disable IDE1006 // Naming Styles
    public virtual void writeLine(string message, TraceEventType eventType) => Write(message + Environment.NewLine, eventType);
#pragma warning restore IDE1006 // Naming Styles
  }
}
