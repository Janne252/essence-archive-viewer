using System;
using System.Globalization;
using System.Windows.Data;

namespace ArchiveViewer.Converters
{
	// Token: 0x0200001A RID: 26
	[ValueConversion(typeof(long), typeof(string))]
	public class LengthConverter : IValueConverter
	{
		// Token: 0x0600009F RID: 159 RVA: 0x00003D28 File Offset: 0x00001F28
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is long)
			{
				long length = (long)value;
				return Convert(length, culture);
			}
			return value;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00003D4E File Offset: 0x00001F4E
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00003D58 File Offset: 0x00001F58
		public static string Convert(long length, CultureInfo culture)
		{
			if (Math.Abs(length) >= 1073741824L)
			{
				return string.Format(culture, "{0:#,##0.00} GB", new object[]
				{
					(double)length / 1073741824.0
				});
			}
			if (Math.Abs(length) >= 1048576L)
			{
				return string.Format(culture, "{0:#,##0.00} MB", new object[]
				{
					(double)length / 1048576.0
				});
			}
			if (Math.Abs(length) >= 1024L)
			{
				return string.Format(culture, "{0:#,##0.00} KB", new object[]
				{
					(double)length / 1024.0
				});
			}
			return string.Format(culture, "{0:#,##0} B", new object[]
			{
				length
			});
		}
	}
}
