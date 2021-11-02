using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ArchiveViewer.Converters;
using Essence.Core.IO.Archive;

namespace ArchiveViewer
{
	// Token: 0x02000009 RID: 9
	public partial class PropertiesWindow : Window
	{
		// Token: 0x06000035 RID: 53 RVA: 0x00002A10 File Offset: 0x00000C10
		public PropertiesWindow(INode node)
		{
			Node = node;
			calculateCumulativeProperties(node);
			calculateExtendedProperties(node);
			InitializeComponent();
			Icon = (ImageSource)((FileInfoConverter)FindResource("FileInfoConverter")).Convert(Node, typeof(ImageSource), FileInfoParameter.LargeIcon, CultureInfo.CurrentCulture);
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00002A79 File Offset: 0x00000C79
		// (set) Token: 0x06000037 RID: 55 RVA: 0x00002A81 File Offset: 0x00000C81
		public INode Node { get; private set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002A8A File Offset: 0x00000C8A
		// (set) Token: 0x06000039 RID: 57 RVA: 0x00002A92 File Offset: 0x00000C92
		public long Size { get; private set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002A9B File Offset: 0x00000C9B
		// (set) Token: 0x0600003B RID: 59 RVA: 0x00002AA3 File Offset: 0x00000CA3
		public long SizeOnDisk { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002AAC File Offset: 0x00000CAC
		// (set) Token: 0x0600003D RID: 61 RVA: 0x00002AB4 File Offset: 0x00000CB4
		public long Files { get; private set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600003E RID: 62 RVA: 0x00002ABD File Offset: 0x00000CBD
		// (set) Token: 0x0600003F RID: 63 RVA: 0x00002AC5 File Offset: 0x00000CC5
		public long Folders { get; private set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00002ACE File Offset: 0x00000CCE
		// (set) Token: 0x06000041 RID: 65 RVA: 0x00002AD6 File Offset: 0x00000CD6
		public ReadOnlyCollection<ExtendedProperty> ExtendedProperties { get; private set; }

		// Token: 0x06000042 RID: 66 RVA: 0x00002AE0 File Offset: 0x00000CE0
		private void calculateCumulativeProperties(INode node)
		{
			File file = node as File;
			if (file != null)
			{
				Size += (long)((ulong)file.StoreLength);
				SizeOnDisk += (long)((ulong)file.Length);
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
				foreach (INode node2 in node.Children)
				{
					calculateCumulativeProperties(node2);
				}
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002BA4 File Offset: 0x00000DA4
		private void calculateExtendedProperties(INode node)
		{
			List<ExtendedProperty> list = new List<ExtendedProperty>();
			if (node is Archive)
			{
				Archive archive = (Archive)node;
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
					list.Add(new ExtendedProperty("Block Size", Converters.LengthConverter.Convert((long)((ulong)archive.BlockSize), CultureInfo.CurrentCulture)));
				}
			}
			else if (node is File)
			{
				File file = (File)node;
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

		// Token: 0x06000044 RID: 68 RVA: 0x00002D18 File Offset: 0x00000F18
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = new bool?(true);
		}

		// Token: 0x0200000A RID: 10
		public class ExtendedProperty
		{
			// Token: 0x06000047 RID: 71 RVA: 0x00002DA5 File Offset: 0x00000FA5
			public ExtendedProperty(string name, object value)
			{
				Name = name;
				Value = value;
			}

			// Token: 0x17000012 RID: 18
			// (get) Token: 0x06000048 RID: 72 RVA: 0x00002DBB File Offset: 0x00000FBB
			// (set) Token: 0x06000049 RID: 73 RVA: 0x00002DC3 File Offset: 0x00000FC3
			public string Name { get; private set; }

			// Token: 0x17000013 RID: 19
			// (get) Token: 0x0600004A RID: 74 RVA: 0x00002DCC File Offset: 0x00000FCC
			// (set) Token: 0x0600004B RID: 75 RVA: 0x00002DD4 File Offset: 0x00000FD4
			public object Value { get; private set; }
		}
	}
}
