using System;
using System.Globalization;
using System.Windows.Data;

namespace EssenceArchiveViewer.Converters
{
	[ValueConversion(typeof(long), typeof(string))]
	public class LengthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is long length)
			{
                return Convert(length, culture);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public static string Convert(long length, CultureInfo culture)
		{
            return Math.Abs(length) switch
            {
                >= 1073741824L => string.Format(culture, "{0:#,##0.00} GB", new object[] {length / 1073741824.0}),
                >= 1048576L => string.Format(culture, "{0:#,##0.00} MB", new object[] {length / 1048576.0}),
                >= 1024L => string.Format(culture, "{0:#,##0.00} KB", new object[] {length / 1024.0}),
                _ => string.Format(culture, "{0:#,##0} B", new object[] {length})
            };
        }
	}
}
