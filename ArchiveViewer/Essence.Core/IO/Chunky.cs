// Decompiled with JetBrains decompiler
// Type: Essence.Core.IO.Chunky
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Text;

namespace Essence.Core.IO
{
  internal class Chunky
  {
    public const string Signature = "Relic Chunky\r\n\u001A\0";
    public const uint CurrentVersion = 4;
    public const uint Platform = 1;
    public static readonly FourCC DataType = FourCC.Parse("DATA");
    public static readonly FourCC FolderType = FourCC.Parse("FOLD");
    internal static readonly Encoding Encoding = (Encoding) new UTF8Encoding(false, true);
  }
}
