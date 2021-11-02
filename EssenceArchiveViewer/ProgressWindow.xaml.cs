using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Essence.Core.IO.Archive;
using File = Essence.Core.IO.Archive.File;

namespace ArchiveViewer
{
    public partial class ProgressWindow : Window, INotifyPropertyChanged
	{
        public ProgressWindow(INode node, string destination)
		{
			Node = node;
			Destination = destination;
			Total = CalculateTotal(node);
			Extracted = 0L;
			InitializeComponent();
		}

		public INode Node { get; private set; }

		public string Destination { get; private set; }

		public long Total { get; private set; }

		public long Extracted { get; private set; }

		public long Remaining => Total - Extracted;

        public double Progress { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		private long CalculateTotal(INode node)
		{
			var num = 0L;
            if (node is File file)
			{
				num += file.StoreLength;
			}
			if (node.Children != null)
			{
				foreach (var node2 in node.Children)
				{
					num += CalculateTotal(node2);
				}
			}
			return num;
		}

		private void NotifyPropertyChanged(params string[] propertyNames)
		{
			var propertyChanged = PropertyChanged;
			if (propertyChanged != null)
			{
				foreach (var propertyName in propertyNames)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		public bool Extract(INode node, string destination)
		{
			if (_cancellationTokenSource.IsCancellationRequested)
			{
				return false;
			}

            if (node is File file)
			{
				System.IO.File.WriteAllBytes(Path.Combine(destination, file.Name), file.GetData());
				Extracted += file.StoreLength;
				Progress = Extracted / (double)Total;
				Dispatcher.Invoke(new Action<string[]>(NotifyPropertyChanged), new object[]
				{
					new[]
					{
						"Extracted",
						"Remaining",
						"Progress"
					}
				});
			}
			if (node.Children != null)
			{
				destination = Path.Combine(destination, node.Name);
				Directory.CreateDirectory(destination);
				foreach (var node2 in node.Children)
				{
					if (!Extract(node2, destination))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		private void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_thread = new Thread(delegate()
			{
				try
				{
					Directory.CreateDirectory(Destination);
					Extract(Node, Destination);
					Dispatcher.Invoke(new Action(delegate
                    {
						DialogResult = true;
					}), Array.Empty<object>());
				}
				catch (Exception exception)
				{
					Dispatcher.Invoke(new Action(delegate
                    {
						MessageBox.Show(string.Format("Error extracting {1}:{0}{0}{2}", Environment.NewLine, Node.Name, exception.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
						DialogResult = false;
					}), Array.Empty<object>());
				}
			})
			{
				Name = "ExtractThread",
				IsBackground = true
			};
			_thread.Start();
		}

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_thread != null)
			{
				_cancellationTokenSource.Cancel();
				_thread.Join();
				_cancellationTokenSource = null;
				_thread = null;
			}
			DialogResult = false;
		}

        private CancellationTokenSource _cancellationTokenSource;

		private Thread _thread;
	}
}
