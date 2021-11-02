// Decompiled with JetBrains decompiler
// Type: Essence.Core.Diagnostics.ILog
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

namespace Essence.Core.Diagnostics
{
  public interface ILog
  {
    void TraceError(string message);

    void TraceError(string format, params object[] args);

    void TraceInformation(string message);

    void TraceInformation(string format, params object[] args);

    void TraceVerbose(string message);

    void TraceVerbose(string format, params object[] args);

    void TraceWarning(string message);

    void TraceWarning(string format, params object[] args);
  }
}
