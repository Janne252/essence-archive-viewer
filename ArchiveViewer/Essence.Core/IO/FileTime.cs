using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Essence.Core.IO
{
  public static class FileTime
  {
    public static uint GetCreationTime(string fileName) => GetTime(fileName, TimeAttribute.CreationTime);

    public static uint GetLastAccessTime(string fileName) => GetTime(fileName, TimeAttribute.LastAccessTime);

    public static uint GetLastWriteTime(string fileName) => GetTime(fileName, TimeAttribute.LastWriteTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetFileAttributesEx(
      string lpFileName,
      GET_FILEEX_INFO_LEVELS fInfoLevelId,
      out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

    private static uint GetTime(string fileName, TimeAttribute timeAttribute)
    {
      WIN32_FILE_ATTRIBUTE_DATA lpFileInformation;
      if (!GetFileAttributesEx(fileName, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out lpFileInformation))
        return 0;
      long dateTime1;
      switch (timeAttribute)
      {
        case TimeAttribute.CreationTime:
          dateTime1 = lpFileInformation.ftCreationTime.GetDateTime();
          break;
        case TimeAttribute.LastAccessTime:
          dateTime1 = lpFileInformation.ftLastAccessTime.GetDateTime();
          break;
        case TimeAttribute.LastWriteTime:
          dateTime1 = lpFileInformation.ftLastWriteTime.GetDateTime();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof (timeAttribute));
      }
      DateTime dateTime2 = DateTime.FromFileTimeUtc(dateTime1);
      DaylightTime daylightChanges = TimeZone.CurrentTimeZone.GetDaylightChanges(dateTime2.Year);
      if (dateTime2 <= daylightChanges.Start || dateTime2 >= daylightChanges.End)
        dateTime1 += daylightChanges.Delta.Ticks;
      return (uint) ((ulong) (dateTime1 - 116444736000000000L) / 10000000UL);
    }

    private enum GET_FILEEX_INFO_LEVELS
    {
      GetFileExInfoStandard,
      GetFileExMaxInfoLevel,
    }

    private struct FILETIME
    {
      private uint dwLowDateTime;
      private uint dwHighDateTime;

      public long GetDateTime() => (long) dwHighDateTime << 32 | (long) dwLowDateTime;
    }

    private struct WIN32_FILE_ATTRIBUTE_DATA
    {
      public uint dwFileAttributes;
      public FILETIME ftCreationTime;
      public FILETIME ftLastAccessTime;
      public FILETIME ftLastWriteTime;
      public ulong nFileSize;
    }

    private enum TimeAttribute
    {
      CreationTime,
      LastAccessTime,
      LastWriteTime,
    }
  }
}
