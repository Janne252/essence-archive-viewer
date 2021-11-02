using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace EssenceArchiveViewer.Converters
{
	[ValueConversion(typeof(INode), typeof(string))]
	public class SizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (value is File file)
			{
				return LengthConverter.Convert(file.StoreLength, culture);
			}

            if (value is INode node)
			{
				var num = node.Children?.Count ?? 0;
				return string.Format(culture, "{0} Item{1}", new object[]
				{
					num,
					(num == 1) ? "" : "s"
				});
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
