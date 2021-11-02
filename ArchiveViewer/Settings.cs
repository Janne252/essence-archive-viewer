using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace ArchiveViewer
{
    [DataContract]
	public sealed class Settings : INotifyPropertyChanged
	{
        public Settings()
		{
			_mainWindow = new WindowSettings();
			Validate();
		}

		public WindowSettings MainWindow => _mainWindow;

        public ObservableCollection<string> RecentFiles => _recentFiles;

		public event PropertyChangedEventHandler PropertyChanged;

		private void Validate()
		{
			if (_mainWindow == null)
			{
				_mainWindow = new WindowSettings();
			}
			if (_recentFiles == null)
			{
				_recentFiles = new ObservableCollection<string>();
				return;
			}
			while (_recentFiles.Count > 10)
			{
				_recentFiles.RemoveAt(_recentFiles.Count - 1);
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			Validate();
		}

		public static string GetRootPath(bool create)
		{
			var text = Path.Combine(new[]
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
						Trace.TraceError("Error creating directory {0}: {1}", new object[]
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

		public static string GetSettingsPath(bool create)
		{
			var rootPath = GetRootPath(create);
			if (rootPath == null)
			{
				return null;
			}
			return Path.Combine(rootPath, "Settings.xml");
		}

		public static Settings Load()
		{
			var settingsPath = GetSettingsPath(false);
			if (settingsPath != null)
			{
				try
				{
					var dataContractSerializer = new DataContractSerializer(typeof(Settings));
					var settings = new XmlReaderSettings
					{
						CloseInput = true
					};
                    using var xmlReader = XmlReader.Create(new FileStream(settingsPath, FileMode.Open, FileAccess.Read, FileShare.Read), settings);
                    return (Settings)dataContractSerializer.ReadObject(xmlReader);
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

		public void Save()
		{
			var settingsPath = GetSettingsPath(true);
			if (settingsPath != null)
			{
				try
				{
					var dataContractSerializer = new DataContractSerializer(typeof(Settings));
					var settings = new XmlWriterSettings
					{
						CloseOutput = true,
						Indent = true,
						IndentChars = "\t"
					};
                    using var xmlWriter = XmlWriter.Create(new FileStream(settingsPath, FileMode.Create, FileAccess.Write, FileShare.Read), settings);
                    dataContractSerializer.WriteObject(xmlWriter, this);
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

		private int FindRecentFile(string fileName)
		{
			for (var i = 0; i < _recentFiles.Count; i++)
			{
				if (fileName.Equals(_recentFiles[i], StringComparison.CurrentCultureIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		public void AddRecentFile(string fileName)
		{
			var num = FindRecentFile(fileName);
			if (num != -1)
			{
				_recentFiles.Move(num, 0);
				return;
			}
			_recentFiles.Insert(0, fileName);
			while (_recentFiles.Count > 10)
			{
				_recentFiles.RemoveAt(_recentFiles.Count - 1);
			}
		}

		public void RemoveRecentFile(string fileName)
		{
			var num = FindRecentFile(fileName);
			if (num != -1)
			{
				_recentFiles.RemoveAt(num);
			}
		}

		[DataMember(Name = "MainWindow")]
		private WindowSettings _mainWindow = new();

		[DataMember(Name = "RecentFiles")]
		private ObservableCollection<string> _recentFiles;
	}
}
