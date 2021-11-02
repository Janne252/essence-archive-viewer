namespace Essence.Core.Diagnostics
{
  public enum LogLevels
  {
    None = 0,
    Error = 1,
    Warning = 2,
    Information = 4,
    Default = 7,
    Verbose = 8,
    All = 15, // 0x0000000F
  }
}
