// Decompiled with JetBrains decompiler
// Type: Essence.Core.Diagnostics.TraceLog
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

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

    public TraceLog(LogLevels levels) => this.m_levels = levels;

    public void TraceError(string message)
    {
      if ((this.m_levels & LogLevels.Error) != LogLevels.Error)
        return;
      Trace.TraceError(message);
    }

    public void TraceError(string format, params object[] args)
    {
      if ((this.m_levels & LogLevels.Error) != LogLevels.Error)
        return;
      Trace.TraceError(format, args);
    }

    public void TraceInformation(string message)
    {
      if ((this.m_levels & LogLevels.Information) != LogLevels.Information)
        return;
      Trace.TraceInformation(message);
    }

    public void TraceInformation(string format, params object[] args)
    {
      if ((this.m_levels & LogLevels.Information) != LogLevels.Information)
        return;
      Trace.TraceInformation(format, args);
    }

    public void TraceVerbose(string message)
    {
      if ((this.m_levels & LogLevels.Verbose) != LogLevels.Verbose)
        return;
      Trace.TraceInformation(message);
    }

    public void TraceVerbose(string format, params object[] args)
    {
      if ((this.m_levels & LogLevels.Verbose) != LogLevels.Verbose)
        return;
      Trace.TraceInformation(format, args);
    }

    public void TraceWarning(string message)
    {
      if ((this.m_levels & LogLevels.Warning) != LogLevels.Warning)
        return;
      Trace.TraceWarning(message);
    }

    public void TraceWarning(string format, params object[] args)
    {
      if ((this.m_levels & LogLevels.Warning) != LogLevels.Warning)
        return;
      Trace.TraceWarning(format, args);
    }
  }
}
