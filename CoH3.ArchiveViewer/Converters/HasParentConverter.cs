using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000004 RID: 4
	[ValueConversion(typeof(INode), typeof(bool))]
	public class HasParentConverter : IValueConverter
	{
		// Token: 0x0600000A RID: 10 RVA: 0x0000214C File Offset: 0x0000034C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			INode node = value as INode;
			return node != null && node.Parent != null;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002177 File Offset: 0x00000377
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
