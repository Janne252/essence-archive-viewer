using System.Diagnostics;

namespace Essence.Core.Diagnostics
{
  public class TraceLog : ILog
  {
    private LogLevels m_levels;

    public TraceLog()
      : this(LogLevels.Default)
    {
    }

    public TraceLog(LogLevels levels) => m_levels = levels;

    public void TraceError(string message)
    {
      if ((m_levels & LogLevels.Error) != LogLevels.Error)
        return;
      Trace.TraceError(message);
    }

    public void TraceError(string format, params object[] args)
    {
      if ((m_levels & LogLevels.Error) != LogLevels.Error)
        return;
      Trace.TraceError(format, args);
    }

    public void TraceInformation(string message)
    {
      if ((m_levels & LogLevels.Information) != LogLevels.Information)
        return;
      Trace.TraceInformation(message);
    }

    public void TraceInformation(string format, params object[] args)
    {
      if ((m_levels & LogLevels.Information) != LogLevels.Information)
        return;
      Trace.TraceInformation(format, args);
    }

    public void TraceVerbose(string message)
    {
      if ((m_levels & LogLevels.Verbose) != LogLevels.Verbose)
        return;
      Trace.TraceInformation(message);
    }

    public void TraceVerbose(string format, params object[] args)
    {
      if ((m_levels & LogLevels.Verbose) != LogLevels.Verbose)
        return;
      Trace.TraceInformation(format, args);
    }

    public void TraceWarning(string message)
    {
      if ((m_levels & LogLevels.Warning) != LogLevels.Warning)
        return;
      Trace.TraceWarning(message);
    }

    public void TraceWarning(string format, params object[] args)
    {
      if ((m_levels & LogLevels.Warning) != LogLevels.Warning)
        return;
      Trace.TraceWarning(format, args);
    }
  }
}
