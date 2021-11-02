using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace EssenceArchiveViewer.Converters
{
	[ValueConversion(typeof(INode), typeof(bool))]
	public class HasParentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            return value is INode {Parent: { }};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
