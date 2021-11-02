using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	[ValueConversion(typeof(INode), typeof(Visibility))]
	public class HasChildrenConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            if (value is INode {Children: { }})
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
