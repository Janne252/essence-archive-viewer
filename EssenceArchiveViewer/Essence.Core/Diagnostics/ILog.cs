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
