using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace ArchiveViewer
{
	// Token: 0x0200000C RID: 12
	[DataContract]
	public sealed class Settings : INotifyPropertyChanged
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00002E8A File Offset: 0x0000108A
		public Settings()
		{
			this.mainWindow = new WindowSettings();
			this.validate();
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00002EAE File Offset: 0x000010AE
		public WindowSettings MainWindow
		{
			get
			{
				return this.mainWindow;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00002EB6 File Offset: 0x000010B6
		public ObservableCollection<string> RecentFiles
		{
			get
			{
				return this.recentFiles;
			}
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600005B RID: 91 RVA: 0x00002EC0 File Offset: 0x000010C0
		// (remove) Token: 0x0600005C RID: 92 RVA: 0x00002EF8 File Offset: 0x000010F8
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x0600005D RID: 93 RVA: 0x00002F30 File Offset: 0x00001130
		private void notifyPropertyChanged(params string[] propertyNames)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				foreach (string propertyName in propertyNames)
				{
					propertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00002F68 File Offset: 0x00001168
		private void validate()
		{
			if (this.mainWindow == null)
			{
				this.mainWindow = new WindowSettings();
			}
			if (this.recentFiles == null)
			{
				this.recentFiles = new ObservableCollection<string>();
				return;
			}
			while (this.recentFiles.Count > 10)
			{
				this.recentFiles.RemoveAt(this.recentFiles.Count - 1);
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002FC3 File Offset: 0x000011C3
		[OnDeserialized]
		private void onDeserialized(StreamingContext context)
		{
			this.validate();
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002FCC File Offset: 0x000011CC
		public static string GetRootPath(bool create)
		{
			string text = Path.Combine(new string[]
			{
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"SEGA",
				"Relic Entertainment",
				"Company of Heroes 2",
				"Archive Viewer"
			});
			if (!Directory.Exists(text))
			{
				if (create)
				{
					try
					{
						Directory.CreateDirectory(text);
						return text;
					}
					catch (Exception ex)
					{
						Trace.TraceError("Error creating directroy {0}: {1}", new object[]
						{
							text,
							ex.Message
						});
						return null;
					}
				}
				return null;
			}
			return text;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003064 File Offset: 0x00001264
		public static string GetSettingsPath(bool create)
		{
			string rootPath = Settings.GetRootPath(create);
			if (rootPath == null)
			{
				return null;
			}
			return Path.Combine(rootPath, "Settings.xml");
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003088 File Offset: 0x00001288
		public static Settings Load()
		{
			string settingsPath = Settings.GetSettingsPath(false);
			if (settingsPath != null)
			{
				try
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Settings));
					XmlReaderSettings settings = new XmlReaderSettings
					{
						CloseInput = true
					};
					using (XmlReader xmlReader = XmlReader.Create(new FileStream(settingsPath, FileMode.Open, FileAccess.Read, FileShare.Read), settings))
					{
						return (Settings)dataContractSerializer.ReadObject(xmlReader);
					}
				}
				catch (FileNotFoundException)
				{
				}
				catch (Exception ex)
				{
					Trace.TraceError("Error loading settings {0}: {1}", new object[]
					{
						settingsPath,
						ex.Message
					});
				}
			}
			return new Settings();
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003148 File Offset: 0x00001348
		public void Save()
		{
			string settingsPath = Settings.GetSettingsPath(true);
			if (settingsPath != null)
			{
				try
				{
					DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Settings));
					XmlWriterSettings settings = new XmlWriterSettings
					{
						CloseOutput = true,
						Indent = true,
						IndentChars = "\t"
					};
					using (XmlWriter xmlWriter = XmlWriter.Create(new FileStream(settingsPath, FileMode.Create, FileAccess.Write, FileShare.Read), settings))
					{
						dataContractSerializer.WriteObject(xmlWriter, this);
					}
				}
				catch (Exception ex)
				{
					Trace.TraceError("Error saving settings {0}: {1}", new object[]
					{
						settingsPath,
						ex.Message
					});
				}
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003204 File Offset: 0x00001404
		private int findRecentFile(string fileName)
		{
			for (int i = 0; i < this.recentFiles.Count; i++)
			{
				if (fileName.Equals(this.recentFiles[i], StringComparison.CurrentCultureIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00003240 File Offset: 0x00001440
		public void AddRecentFile(string fileName)
		{
			int num = this.findRecentFile(fileName);
			if (num != -1)
			{
				this.recentFiles.Move(num, 0);
				return;
			}
			this.recentFiles.Insert(0, fileName);
			while (this.recentFiles.Count > 10)
			{
				this.recentFiles.RemoveAt(this.recentFiles.Count - 1);
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000032A0 File Offset: 0x000014A0
		public void RemoveRecentFile(string fileName)
		{
			int num = this.findRecentFile(fileName);
			if (num != -1)
			{
				this.recentFiles.RemoveAt(num);
			}
		}

		// Token: 0x04000028 RID: 40
		private const int MAX_RECENT_FILES = 10;

		// Token: 0x04000029 RID: 41
		[DataMember(Name = "MainWindow")]
		private WindowSettings mainWindow = new WindowSettings();

		// Token: 0x0400002A RID: 42
		[DataMember(Name = "RecentFiles")]
		private ObservableCollection<string> recentFiles;
	}
}
