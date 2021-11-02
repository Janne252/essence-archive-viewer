using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000003 RID: 3
	[ValueConversion(typeof(INode), typeof(Visibility))]
	public class HasChildrenConverter : IValueConverter
	{
		// Token: 0x06000007 RID: 7 RVA: 0x00002110 File Offset: 0x00000310
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			INode node = value as INode;
			if (node != null && node.Children != null)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000213C File Offset: 0x0000033C
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
