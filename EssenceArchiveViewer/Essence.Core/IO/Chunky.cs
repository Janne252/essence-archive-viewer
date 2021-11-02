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
    internal static readonly Encoding Encoding = new UTF8Encoding(false, true);
  }
}
