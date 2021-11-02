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
	internal class FileInfoCache
	{
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileAttributes dwFileAttributes, ref SHFileInfo psfi, uint cbFileInfo, [MarshalAs(UnmanagedType.U4)] SHFileInfoFlags uFlags);

		[DllImport("user32.dll")]
		private static extern bool DestroyIcon(IntPtr hIcon);

		private bool GetFileInfo(FileInfoKey fileInfoKey, SHFileInfoFlags fileInfoFlags, out SHFileInfo shFileInfo)
		{
			shFileInfo = default;
			return SHGetFileInfo(fileInfoKey.Path, fileInfoKey.FileAttributes, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), SHFileInfoFlags.SHGFI_USEFILEATTRIBUTES | fileInfoFlags) != IntPtr.Zero;
		}

		private ImageSource GetFileInfoIcon(FileInfoKey fileInfoKey, SHFileInfoFlags fileInfoFlags)
		{
			try
			{
                if (GetFileInfo(fileInfoKey, SHFileInfoFlags.SHGFI_ICON | fileInfoFlags, out var shfileInfo) && shfileInfo.hIcon != IntPtr.Zero)
				{
					try
					{
						return Imaging.CreateBitmapSourceFromHIcon(shfileInfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					}
					finally
					{
						DestroyIcon(shfileInfo.hIcon);
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public FileInfo GetFileInfo(INode node)
		{
			FileInfoKey fileInfoKey = null;
			if (node is Archive or Essence.Core.IO.Archive.File)
			{
				var num = node.Name.LastIndexOf('.');
				fileInfoKey = new FileInfoKey(
                    $"File{((num >= 0) ? node.Name.Substring(num).ToLowerInvariant() : "File")}", FileAttributes.FILE_ATTRIBUTE_NORMAL);
			}
			else if (node is TOC or Folder)
			{
				fileInfoKey = new FileInfoKey("Folder", FileAttributes.FILE_ATTRIBUTE_DIRECTORY);
			}
			if (fileInfoKey == null)
			{
				return null;
			}

            if (fileInfoCache.TryGetValue(fileInfoKey, out var fileInfo))
			{
				return fileInfo;
			}
			string description;
			if ((fileInfoKey.FileAttributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == FileAttributes.FILE_ATTRIBUTE_NONE)
			{
                if (GetFileInfo(fileInfoKey, SHFileInfoFlags.SHGFI_TYPENAME, out var shfileInfo))
				{
					description = shfileInfo.szTypeName;
				}
				else
				{
					var num2 = node.Name.LastIndexOfAny(new[]
					{
						'.',
						Path.DirectorySeparatorChar,
						Path.AltDirectorySeparatorChar
					});
					if (num2 >= 0 && node.Name[num2] == '.' && num2 + 2 < node.Name.Length)
					{
						description = $"{node.Name.Substring(num2 + 1).ToUpperInvariant()} File";
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
			var fileInfoIcon = GetFileInfoIcon(fileInfoKey, SHFileInfoFlags.SHGFI_SMALLICON);
			var fileInfoIcon2 = GetFileInfoIcon(fileInfoKey, SHFileInfoFlags.SHGFI_NONE);
			ImageSource imageSource = null;
			ImageSource imageSource2 = null;
			if ((fileInfoKey.FileAttributes & FileAttributes.FILE_ATTRIBUTE_DIRECTORY) != FileAttributes.FILE_ATTRIBUTE_NONE)
			{
				imageSource = GetFileInfoIcon(fileInfoKey, (SHFileInfoFlags)3u);
				imageSource2 = GetFileInfoIcon(fileInfoKey, SHFileInfoFlags.SHGFI_OPENICON);
			}
			fileInfo = new FileInfo(description, fileInfoIcon ?? fileInfoIcon2, fileInfoIcon2 ?? fileInfoIcon, imageSource ?? imageSource2, imageSource2 ?? imageSource);
			fileInfoCache.Add(fileInfoKey, fileInfo);
			return fileInfo;
		}

		private readonly Dictionary<FileInfoKey, FileInfo> fileInfoCache = new();

		[Flags]
        private enum FileAttributes : uint
		{
			FILE_ATTRIBUTE_NONE,
			FILE_ATTRIBUTE_READONLY,
			FILE_ATTRIBUTE_HIDDEN,
			FILE_ATTRIBUTE_SYSTEM = 4u,
			FILE_ATTRIBUTE_DIRECTORY = 16u,
			FILE_ATTRIBUTE_ARCHIVE = 32u,
			FILE_ATTRIBUTE_DEVICE = 64u,
			FILE_ATTRIBUTE_NORMAL = 128u,
			FILE_ATTRIBUTE_TEMPORARY = 256u,
			FILE_ATTRIBUTE_SPARSE_FILE = 512u,
			FILE_ATTRIBUTE_REPARSE_POINT = 1024u,
			FILE_ATTRIBUTE_COMPRESSED = 2048u,
			FILE_ATTRIBUTE_OFFLINE = 4096u,
			FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192u,
			FILE_ATTRIBUTE_ENCRYPTED = 16384u,
			FILE_ATTRIBUTE_VIRTUAL = 65536u
		}

		private enum SHFileInfoFlags : uint
		{
			SHGFI_NONE,
			SHGFI_ICON = 256u,
			SHGFI_DISPLAYNAME = 512u,
			SHGFI_TYPENAME = 1024u,
			SHGFI_ATTRIBUTES = 2048u,
			SHGFI_ICONLOCATION = 4096u,
			SHGFI_EXETYPE = 8192u,
			SHGFI_SYSICONINDEX = 16384u,
			SHGFI_LINKOVERLAY = 32768u,
			SHGFI_SELECTED = 65536u,
			SHGFI_ATTR_SPECIFIED = 131072u,
			SHGFI_LARGEICON = 0u,
			SHGFI_SMALLICON,
			SHGFI_OPENICON,
			SHGFI_SHELLICONSIZE = 4u,
			SHGFI_PIDL = 8u,
			SHGFI_USEFILEATTRIBUTES = 16u,
			SHGFI_ADDOVERLAYS = 32u,
			SHGFI_OVERLAYINDEX = 64u
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFileInfo
		{
			public readonly IntPtr hIcon;

			public readonly int iIcon;

			public readonly uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public readonly string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public readonly string szTypeName;
		}

		private class FileInfoKey : IEquatable<FileInfoKey>
		{
			public FileInfoKey(string path, FileAttributes fileAttributes)
			{
                Path = path ?? throw new ArgumentNullException("path");
				FileAttributes = fileAttributes;
			}
			
			public string Path { get; }

			public FileAttributes FileAttributes { get; }

			public override bool Equals(object obj)
			{
				return Equals(obj as FileInfoKey);
			}

			public bool Equals(FileInfoKey other)
			{
				return other != null && Path.Equals(other.Path) && FileAttributes == other.FileAttributes;
			}

			public override int GetHashCode()
			{
				return Path.GetHashCode() ^ (int)FileAttributes;
			}

			public override string ToString()
			{
				return Path;
			}
		}

		public class FileInfo
		{
			public FileInfo(string description, ImageSource smallIcon, ImageSource largeIcon, ImageSource smallOpenIcon, ImageSource largeOpenIcon)
			{
				Description = description;
				SmallIcon = smallIcon;
				LargeIcon = largeIcon;
				SmallOpenIcon = (smallOpenIcon ?? smallIcon);
				LargeOpenIcon = (largeOpenIcon ?? largeIcon);
			}
			
			public string Description { get; }
			
			public ImageSource SmallIcon { get; }
			
			public ImageSource LargeIcon { get; }
			
			public ImageSource SmallOpenIcon { get; }
			
			public ImageSource LargeOpenIcon { get; }
		}
	}
}
