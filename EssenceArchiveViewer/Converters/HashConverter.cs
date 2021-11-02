using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ArchiveViewer.Converters
{
	[ValueConversion(typeof(byte[]), typeof(string))]
	public class HashConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (value is byte[] array)
			{
				return Convert(array, culture);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public static string Convert(byte[] hash, CultureInfo culture)
		{
            if (hash == null) return null;

            var stringBuilder = new StringBuilder(2 * hash.Length);
            foreach (var b in hash)
            {
                stringBuilder.AppendFormat(culture, "{0:X2}", new object[]
                {
                    b
                });
            }
            return stringBuilder.ToString();
        }
	}
}
