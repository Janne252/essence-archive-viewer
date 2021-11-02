using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000017 RID: 23
	[ValueConversion(typeof(INode), typeof(object))]
	public class FileInfoConverter : IValueConverter
	{
		// Token: 0x06000095 RID: 149 RVA: 0x00003B28 File Offset: 0x00001D28
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			FileInfoParameter fileInfoParameter = FileInfoParameter.Invalid;
			if (parameter is FileInfoParameter)
			{
				fileInfoParameter = (FileInfoParameter)parameter;
			}
			else if (parameter is string)
			{
				fileInfoParameter = (FileInfoParameter)Enum.Parse(typeof(FileInfoParameter), (string)parameter, true);
			}
			switch (fileInfoParameter)
			{
			case FileInfoParameter.Invalid:
				throw new ArgumentOutOfRangeException("parameter");
			case FileInfoParameter.Description:
			{
				TOC toc = value as TOC;
				if (toc != null)
				{
					return toc.AlternateName;
				}
				break;
			}
			}
			INode node = value as INode;
			if (node != null)
			{
				FileInfoCache.FileInfo fileInfo = this.fileInfoCahce.GetFileInfo(node);
				if (fileInfo != null)
				{
					switch (fileInfoParameter)
					{
					case FileInfoParameter.Description:
						return fileInfo.Description;
					case FileInfoParameter.SmallIcon:
						return fileInfo.SmallIcon;
					case FileInfoParameter.LargeIcon:
						return fileInfo.LargeIcon;
					case FileInfoParameter.SmallOpenIcon:
						return fileInfo.SmallOpenIcon;
					case FileInfoParameter.LargeOpenIcon:
						return fileInfo.LargeOpenIcon;
					}
				}
			}
			return value;
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003BFC File Offset: 0x00001DFC
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000074 RID: 116
		private FileInfoCache fileInfoCahce = new FileInfoCache();
	}
}
