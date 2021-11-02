using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Essence.Core.IO.Archive;

namespace ArchiveViewer.Converters
{
	// Token: 0x02000010 RID: 16
	internal class FileInfoCache
	{
		// Token: 0x0600007B RID: 123
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileInfoCache.FileAttributes dwFileAttributes, ref FileInfoCache.SHFileInfo psfi, uint cbFileInfo, [MarshalAs(UnmanagedType.U4)] FileInfoCache.SHFileInfoFlags uFlags);

		// Token: 0x0600007C RID: 124
		[DllImport("user32.dll")]
		private static extern bool DestroyIcon(IntPtr hIcon);

		// Token: 0x0600007D RID: 125 RVA: 0x0000377C File Offset: 0x0000197C
		private bool GetFileInfo(FileInfoCache.FileInfoKey fileInfoKey, FileInfoCache.SHFileInfoFlags fileInfoFlags, out FileInfoCache.SHFileInfo shFileInfo)
		{
			shFileInfo = default(FileInfoCache.SHFileInfo);
			return FileInfoCache.SHGetFileInfo(fileInfoKey.Path, fileInfoKey.FileAttributes, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), FileInfoCache.SHFileInfoFlags.SHGFI_USEFILEATTRIBUTES | fileInfoFlags) != IntPtr.Zero;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000037B8 File Offset: 0x000019B8
		private ImageSource GetFileInfoIcon(FileInfoCache.FileInfoKey fileInfoKey, FileInfoCache.SHFileInfoFlags fileInfoFlags)
		{
			try
			{
				FileInfoCache.SHFileInfo shfileInfo;
				if (this.GetFileInfo(fileInfoKey, FileInfoCache.SHFileInfoFlags.SHGFI_ICON | fileInfoFlags, out shfileInfo) && shfileInfo.hIcon != IntPtr.Zero)
				{
					try
					{
						return Imaging.CreateBitmapSourceFromHIcon(shfileInfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					}
					finally
					{
						FileInfoCache.DestroyIcon(shfileInfo.hIcon);
					}
				}
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003838 File Offset: 0x00001A38
		public FileInfoCache.FileInfo GetFileInfo(INode node)
		{
			FileInfoCache.FileInfoKey fileInfoKey = null;
			if (node is Archive || node is Essence.Core.IO.Archive.File)
			{
				int num = node.Name.LastIndexOf('.');
				fileInfoKey = new FileInfoCache.FileInfoKey(string.Format("File{0}", (num >= 0) ? node.Name.Substring(num).ToLowerInvariant() : "File"), FileInfoCache.FileAttributes.FILE_ATTRIBUTE_NORMAL);
			}
			else if (node is TOC || node is Folder)
			{
				fileInfoKey = new FileInfoCache.FileInfoKey("Folder", FileInfoCache.FileAttributes.FILE_ATTRIBUTE_DIRECTORY);
			}
			if (fileInfoKey == null)
			{
				return null;
			}
			FileInfoCache.FileInfo fileInfo;
			if (this.fileInfoCache.TryGetValue(fileInfoKey, out fileInfo))
			{
				return fileInfo;
			}
			string description;
			if ((fileInfoKey.FileAttributes & FileInfoCache.FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == FileInfoCache.FileAttributes.FILE_ATTRIBUTE_NONE)
			{
				FileInfoCache.SHFileInfo shfileInfo;
				if (this.GetFileInfo(fileInfoKey, FileInfoCache.SHFileInfoFlags.SHGFI_TYPENAME, out shfileInfo))
				{
					description = shfileInfo.szTypeName;
				}
				else
				{
					int num2 = node.Name.LastIndexOfAny(new char[]
					{
						'.',
						Path.DirectorySeparatorChar,
						Path.AltDirectorySeparatorChar
					});
					if (num2 >= 0 && node.Name[num2] == '.' && num2 + 2 < node.Name.Length)
					{
						description = string.Format("{0} File", node.Name.Substring(num2 + 1).ToUpperInvariant());
					}
					else
					{
						description = "File";
					}
				}
			}
			else
			{
				description = "File Folder";
			}
			ImageSource fileInfoIcon = this.GetFileInfoIcon(fileInfoKey, FileInfoCache.SHFileInfoFlags.SHGFI_SMALLICON);
			ImageSource fileInfoIcon2 = this.GetFileInfoIcon(fileInfoKey, FileInfoCache.SHFileInfoFlags.SHGFI_NONE);
			ImageSource imageSource = null;
			ImageSource imageSource2 = null;
			if ((fileInfoKey.FileAttributes & FileInfoCache.FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != FileInfoCache.FileAttributes.FILE_ATTRIBUTE_NONE)
			{
				imageSource = this.GetFileInfoIcon(fileInfoKey, (FileInfoCache.SHFileInfoFlags)3u);
				imageSource2 = this.GetFileInfoIcon(fileInfoKey, FileInfoCache.SHFileInfoFlags.SHGFI_OPENICON);
			}
			fileInfo = new FileInfoCache.FileInfo(description, fileInfoIcon ?? fileInfoIcon2, fileInfoIcon2 ?? fileInfoIcon, imageSource ?? imageSource2, imageSource2 ?? imageSource);
			this.fileInfoCache.Add(fileInfoKey, fileInfo);
			return fileInfo;
		}

		// Token: 0x0400003B RID: 59
		private Dictionary<FileInfoCache.FileInfoKey, FileInfoCache.FileInfo> fileInfoCache = new Dictionary<FileInfoCache.FileInfoKey, FileInfoCache.FileInfo>();

		// Token: 0x02000011 RID: 17
		private enum FileAttributes : uint
		{
			// Token: 0x0400003D RID: 61
			FILE_ATTRIBUTE_NONE,
			// Token: 0x0400003E RID: 62
			FILE_ATTRIBUTE_READONLY,
			// Token: 0x0400003F RID: 63
			FILE_ATTRIBUTE_HIDDEN,
			// Token: 0x04000040 RID: 64
			FILE_ATTRIBUTE_SYSTEM = 4u,
			// Token: 0x04000041 RID: 65
			FILE_ATTRIBUTE_DIRECTORY = 16u,
			// Token: 0x04000042 RID: 66
			FILE_ATTRIBUTE_ARCHIVE = 32u,
			// Token: 0x04000043 RID: 67
			FILE_ATTRIBUTE_DEVICE = 64u,
			// Token: 0x04000044 RID: 68
			FILE_ATTRIBUTE_NORMAL = 128u,
			// Token: 0x04000045 RID: 69
			FILE_ATTRIBUTE_TEMPORARY = 256u,
			// Token: 0x04000046 RID: 70
			FILE_ATTRIBUTE_SPARSE_FILE = 512u,
			// Token: 0x04000047 RID: 71
			FILE_ATTRIBUTE_REPARSE_POINT = 1024u,
			// Token: 0x04000048 RID: 72
			FILE_ATTRIBUTE_COMPRESSED = 2048u,
			// Token: 0x04000049 RID: 73
			FILE_ATTRIBUTE_OFFLINE = 4096u,
			// Token: 0x0400004A RID: 74
			FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192u,
			// Token: 0x0400004B RID: 75
			FILE_ATTRIBUTE_ENCRYPTED = 16384u,
			// Token: 0x0400004C RID: 76
			FILE_ATTRIBUTE_VIRTUAL = 65536u
		}

		// Token: 0x02000012 RID: 18
		private enum SHFileInfoFlags : uint
		{
			// Token: 0x0400004E RID: 78
			SHGFI_NONE,
			// Token: 0x0400004F RID: 79
			SHGFI_ICON = 256u,
			// Token: 0x04000050 RID: 80
			SHGFI_DISPLAYNAME = 512u,
			// Token: 0x04000051 RID: 81
			SHGFI_TYPENAME = 1024u,
			// Token: 0x04000052 RID: 82
			SHGFI_ATTRIBUTES = 2048u,
			// Token: 0x04000053 RID: 83
			SHGFI_ICONLOCATION = 4096u,
			// Token: 0x04000054 RID: 84
			SHGFI_EXETYPE = 8192u,
			// Token: 0x04000055 RID: 85
			SHGFI_SYSICONINDEX = 16384u,
			// Token: 0x04000056 RID: 86
			SHGFI_LINKOVERLAY = 32768u,
			// Token: 0x04000057 RID: 87
			SHGFI_SELECTED = 65536u,
			// Token: 0x04000058 RID: 88
			SHGFI_ATTR_SPECIFIED = 131072u,
			// Token: 0x04000059 RID: 89
			SHGFI_LARGEICON = 0u,
			// Token: 0x0400005A RID: 90
			SHGFI_SMALLICON,
			// Token: 0x0400005B RID: 91
			SHGFI_OPENICON,
			// Token: 0x0400005C RID: 92
			SHGFI_SHELLICONSIZE = 4u,
			// Token: 0x0400005D RID: 93
			SHGFI_PIDL = 8u,
			// Token: 0x0400005E RID: 94
			SHGFI_USEFILEATTRIBUTES = 16u,
			// Token: 0x0400005F RID: 95
			SHGFI_ADDOVERLAYS = 32u,
			// Token: 0x04000060 RID: 96
			SHGFI_OVERLAYINDEX = 64u
		}

		// Token: 0x02000013 RID: 19
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFileInfo
		{
			// Token: 0x04000061 RID: 97
			public IntPtr hIcon;

			// Token: 0x04000062 RID: 98
			public int iIcon;

			// Token: 0x04000063 RID: 99
			public uint dwAttributes;

			// Token: 0x04000064 RID: 100
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;

			// Token: 0x04000065 RID: 101
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		// Token: 0x02000014 RID: 20
		private class FileInfoKey : IEquatable<FileInfoCache.FileInfoKey>
		{
			// Token: 0x06000081 RID: 129 RVA: 0x000039FF File Offset: 0x00001BFF
			public FileInfoKey(string path, FileInfoCache.FileAttributes fileAttributes)
			{
				if (path == null)
				{
					throw new ArgumentNullException("path");
				}
				this.Path = path;
				this.FileAttributes = fileAttributes;
			}

			// Token: 0x17000020 RID: 32
			// (get) Token: 0x06000082 RID: 130 RVA: 0x00003A23 File Offset: 0x00001C23
			// (set) Token: 0x06000083 RID: 131 RVA: 0x00003A2B File Offset: 0x00001C2B
			public string Path { get; private set; }

			// Token: 0x17000021 RID: 33
			// (get) Token: 0x06000084 RID: 132 RVA: 0x00003A34 File Offset: 0x00001C34
			// (set) Token: 0x06000085 RID: 133 RVA: 0x00003A3C File Offset: 0x00001C3C
			public FileInfoCache.FileAttributes FileAttributes { get; private set; }

			// Token: 0x06000086 RID: 134 RVA: 0x00003A45 File Offset: 0x00001C45
			public override bool Equals(object obj)
			{
				return this.Equals(obj as FileInfoCache.FileInfoKey);
			}

			// Token: 0x06000087 RID: 135 RVA: 0x00003A53 File Offset: 0x00001C53
			public bool Equals(FileInfoCache.FileInfoKey other)
			{
				return other != null && this.Path.Equals(other.Path) && this.FileAttributes == other.FileAttributes;
			}

			// Token: 0x06000088 RID: 136 RVA: 0x00003A7D File Offset: 0x00001C7D
			public override int GetHashCode()
			{
				return this.Path.GetHashCode() ^ (int)this.FileAttributes;
			}

			// Token: 0x06000089 RID: 137 RVA: 0x00003A91 File Offset: 0x00001C91
			public override string ToString()
			{
				return this.Path;
			}
		}

		// Token: 0x02000015 RID: 21
		public class FileInfo
		{
			// Token: 0x0600008A RID: 138 RVA: 0x00003A99 File Offset: 0x00001C99
			public FileInfo(string description, ImageSource smallIcon, ImageSource largeIcon, ImageSource smallOpenIcon, ImageSource largeOpenIcon)
			{
				this.Description = description;
				this.SmallIcon = smallIcon;
				this.LargeIcon = largeIcon;
				this.SmallOpenIcon = (smallOpenIcon ?? smallIcon);
				this.LargeOpenIcon = (largeOpenIcon ?? largeIcon);
			}

			// Token: 0x17000022 RID: 34
			// (get) Token: 0x0600008B RID: 139 RVA: 0x00003AD0 File Offset: 0x00001CD0
			// (set) Token: 0x0600008C RID: 140 RVA: 0x00003AD8 File Offset: 0x00001CD8
			public string Description { get; private set; }

			// Token: 0x17000023 RID: 35
			// (get) Token: 0x0600008D RID: 141 RVA: 0x00003AE1 File Offset: 0x00001CE1
			// (set) Token: 0x0600008E RID: 142 RVA: 0x00003AE9 File Offset: 0x00001CE9
			public ImageSource SmallIcon { get; private set; }

			// Token: 0x17000024 RID: 36
			// (get) Token: 0x0600008F RID: 143 RVA: 0x00003AF2 File Offset: 0x00001CF2
			// (set) Token: 0x06000090 RID: 144 RVA: 0x00003AFA File Offset: 0x00001CFA
			public ImageSource LargeIcon { get; private set; }

			// Token: 0x17000025 RID: 37
			// (get) Token: 0x06000091 RID: 145 RVA: 0x00003B03 File Offset: 0x00001D03
			// (set) Token: 0x06000092 RID: 146 RVA: 0x00003B0B File Offset: 0x00001D0B
			public ImageSource SmallOpenIcon { get; private set; }

			// Token: 0x17000026 RID: 38
			// (get) Token: 0x06000093 RID: 147 RVA: 0x00003B14 File Offset: 0x00001D14
			// (set) Token: 0x06000094 RID: 148 RVA: 0x00003B1C File Offset: 0x00001D1C
			public ImageSource LargeOpenIcon { get; private set; }
		}
	}
}
