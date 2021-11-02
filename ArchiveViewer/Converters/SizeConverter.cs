using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000019 RID: 25
	[ValueConversion(typeof(INode), typeof(string))]
	public class SizeConverter : IValueConverter
	{
		// Token: 0x0600009C RID: 156 RVA: 0x00003CA0 File Offset: 0x00001EA0
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			File file = value as File;
			if (file != null)
			{
				return LengthConverter.Convert((long)((ulong)file.StoreLength), culture);
			}
			INode node = value as INode;
			if (node != null)
			{
				int num = (node.Children != null) ? node.Children.Count : 0;
				return string.Format(culture, "{0} Item{1}", new object[]
				{
					num,
					(num == 1) ? "" : "s"
				});
			}
			return value;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003D19 File Offset: 0x00001F19
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
