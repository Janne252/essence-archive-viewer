// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.FileTime
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Essence.Core.IO
{
  public static class FileTime
  {
    public static uint GetCreationTime(string fileName) => FileTime.GetTime(fileName, FileTime.TimeAttribute.CreationTime);

    public static uint GetLastAccessTime(string fileName) => FileTime.GetTime(fileName, FileTime.TimeAttribute.LastAccessTime);

    public static uint GetLastWriteTime(string fileName) => FileTime.GetTime(fileName, FileTime.TimeAttribute.LastWriteTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetFileAttributesEx(
      string lpFileName,
      FileTime.GET_FILEEX_INFO_LEVELS fInfoLevelId,
      out FileTime.WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

    private static uint GetTime(string fileName, FileTime.TimeAttribute timeAttribute)
    {
      FileTime.WIN32_FILE_ATTRIBUTE_DATA lpFileInformation;
      if (!FileTime.GetFileAttributesEx(fileName, FileTime.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out lpFileInformation))
        return 0;
      long dateTime1;
      switch (timeAttribute)
      {
        case FileTime.TimeAttribute.CreationTime:
          dateTime1 = lpFileInformation.ftCreationTime.GetDateTime();
          break;
        case FileTime.TimeAttribute.LastAccessTime:
          dateTime1 = lpFileInformation.ftLastAccessTime.GetDateTime();
          break;
        case FileTime.TimeAttribute.LastWriteTime:
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

      public long GetDateTime() => (long) this.dwHighDateTime << 32 | (long) this.dwLowDateTime;
    }

    private struct WIN32_FILE_ATTRIBUTE_DATA
    {
      public uint dwFileAttributes;
      public FileTime.FILETIME ftCreationTime;
      public FileTime.FILETIME ftLastAccessTime;
      public FileTime.FILETIME ftLastWriteTime;
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
