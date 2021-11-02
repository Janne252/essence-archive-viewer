using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArchiveViewer.Converters;
using Essence.Core.IO.Archive;

namespace ArchiveViewer
{
    public partial class PropertiesWindow : Window
	{
        public PropertiesWindow(INode node)
		{
			Node = node;
			CalculateCumulativeProperties(node);
			CalculateExtendedProperties(node);
			InitializeComponent();
			Icon = (ImageSource)((FileInfoConverter)FindResource("FileInfoConverter")).Convert(Node, typeof(ImageSource), FileInfoParameter.LargeIcon, CultureInfo.CurrentCulture);
		}

		public INode Node { get; }

		public long Size { get; private set; }

		public long SizeOnDisk { get; private set; }

		public long Files { get; private set; }

		public long Folders { get; private set; }

		public ReadOnlyCollection<ExtendedProperty> ExtendedProperties { get; private set; }

		private void CalculateCumulativeProperties(INode node)
		{
            if (node is File file)
			{
				Size += file.StoreLength;
				SizeOnDisk += file.Length;
				if (!ReferenceEquals(Node, node))
				{
					Files += 1L;
				}
			}
			else if (!ReferenceEquals(Node, node))
			{
				Folders += 1L;
			}
			if (node.Children != null)
			{
				foreach (var node2 in node.Children)
				{
					CalculateCumulativeProperties(node2);
				}
			}
		}
        
        private void CalculateExtendedProperties(INode node)
		{
			var list = new List<ExtendedProperty>();
			if (node is Archive archive)
			{
                list.Add(new ExtendedProperty("Name", archive.NiceName));
				list.Add(new ExtendedProperty("Version", archive.Version));
				list.Add(new ExtendedProperty("Product", archive.Product));
				if (archive.Version <= 5)
				{
					list.Add(new ExtendedProperty("Archive Hash", HashConverter.Convert(archive.FileMD5, CultureInfo.CurrentCulture)));
					list.Add(new ExtendedProperty("Header Hash", HashConverter.Convert(archive.HeaderMD5, CultureInfo.CurrentCulture)));
				}
				if (archive.Version >= 7)
				{
					list.Add(new ExtendedProperty("Block Size", Converters.LengthConverter.Convert(archive.BlockSize, CultureInfo.CurrentCulture)));
				}
			}
			else if (node is File file)
			{
                try
				{
					list.Add(new ExtendedProperty("CRC", HashConverter.Convert(BitConverter.GetBytes(file.CRC32), CultureInfo.CurrentCulture)));
				}
				catch (Exception)
				{
				}
				list.Add(new ExtendedProperty("Verification Type", file.VerificationType));
				list.Add(new ExtendedProperty("Storage Type", file.StorageType));
			}
			ExtendedProperties = new ReadOnlyCollection<ExtendedProperty>(list);
		}

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
		}

		public class ExtendedProperty
		{
			public ExtendedProperty(string name, object value)
			{
				Name = name;
				Value = value;
			}

			public string Name { get; }

			public object Value { get; }
		}
	}
}
