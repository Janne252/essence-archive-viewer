using System;
using System.Globalization;
using System.Windows.Data;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	[ValueConversion(typeof(INode), typeof(object))]
	public class FileInfoConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            var fileInfoParameter = parameter switch
            {
                FileInfoParameter infoParameter => infoParameter,
                string s => (FileInfoParameter) Enum.Parse(typeof(FileInfoParameter), s, true),
                _ => FileInfoParameter.Invalid
            };
            switch (fileInfoParameter)
			{
			case FileInfoParameter.Invalid:
				throw new ArgumentOutOfRangeException(nameof(parameter));
			case FileInfoParameter.Description:
			{
                if (value is TOC toc)
				{
					return toc.AlternateName;
				}
				break;
			}
			}

            if (value is INode node)
			{
				var fileInfo = _fileInfoCache.GetFileInfo(node);
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

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private readonly FileInfoCache _fileInfoCache = new();
	}
}
