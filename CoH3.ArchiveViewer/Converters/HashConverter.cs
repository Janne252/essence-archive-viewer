using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000018 RID: 24
	[ValueConversion(typeof(byte[]), typeof(string))]
	public class HashConverter : IValueConverter
	{
		// Token: 0x06000098 RID: 152 RVA: 0x00003C18 File Offset: 0x00001E18
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			byte[] array = value as byte[];
			if (array != null)
			{
				return HashConverter.Convert(array, culture);
			}
			return value;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003C39 File Offset: 0x00001E39
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003C40 File Offset: 0x00001E40
		public static string Convert(byte[] hash, CultureInfo culture)
		{
			if (hash != null)
			{
				StringBuilder stringBuilder = new StringBuilder(2 * hash.Length);
				foreach (byte b in hash)
				{
					stringBuilder.AppendFormat(culture, "{0:X2}", new object[]
					{
						b
					});
				}
				return stringBuilder.ToString();
			}
			return null;
		}
	}
}
