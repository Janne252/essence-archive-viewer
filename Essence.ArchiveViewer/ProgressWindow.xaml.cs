using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Essence.Core.IO.Archive;

namespace ArchiveViewer
{
	// Token: 0x02000008 RID: 8
	public partial class ProgressWindow : Window, INotifyPropertyChanged
	{
		// Token: 0x0600001E RID: 30 RVA: 0x00002511 File Offset: 0x00000711
		public ProgressWindow(INode node, string destination)
		{
			this.Node = node;
			this.Destination = destination;
			this.Total = this.calculateTotal(node);
			this.Extracted = 0L;
			this.InitializeComponent();
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002542 File Offset: 0x00000742
		// (set) Token: 0x06000020 RID: 32 RVA: 0x0000254A File Offset: 0x0000074A
		public INode Node { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002553 File Offset: 0x00000753
		// (set) Token: 0x06000022 RID: 34 RVA: 0x0000255B File Offset: 0x0000075B
		public string Destination { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002564 File Offset: 0x00000764
		// (set) Token: 0x06000024 RID: 36 RVA: 0x0000256C File Offset: 0x0000076C
		public long Total { get; private set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002575 File Offset: 0x00000775
		// (set) Token: 0x06000026 RID: 38 RVA: 0x0000257D File Offset: 0x0000077D
		public long Extracted { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002586 File Offset: 0x00000786
		public long Remaining
		{
			get
			{
				return this.Total - this.Extracted;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00002595 File Offset: 0x00000795
		// (set) Token: 0x06000029 RID: 41 RVA: 0x0000259D File Offset: 0x0000079D
		public double Progress { get; private set; }

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600002A RID: 42 RVA: 0x000025A8 File Offset: 0x000007A8
		// (remove) Token: 0x0600002B RID: 43 RVA: 0x000025E0 File Offset: 0x000007E0
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x0600002C RID: 44 RVA: 0x00002618 File Offset: 0x00000818
		private long calculateTotal(INode node)
		{
			long num = 0L;
            Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
			if (file != null)
			{
				num += (long)((ulong)file.StoreLength);
			}
			if (node.Children != null)
			{
				foreach (INode node2 in node.Children)
				{
					num += this.calculateTotal(node2);
				}
			}
			return num;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002688 File Offset: 0x00000888
		private void notifyPropertyChanged(params string[] propertyNames)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				foreach (string propertyName in propertyNames)
				{
					this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000026C8 File Offset: 0x000008C8
		public bool Extract(INode node, string destination)
		{
			if (this.cancellationTokenSource.IsCancellationRequested)
			{
				return false;
			}
            Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
			if (file != null)
			{
				System.IO.File.WriteAllBytes(Path.Combine(destination, file.Name), file.GetData());
				this.Extracted += (long)((ulong)file.StoreLength);
				this.Progress = (double)this.Extracted / (double)this.Total;
				base.Dispatcher.Invoke(new Action<string[]>(this.notifyPropertyChanged), new object[]
				{
					new string[]
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
				foreach (INode node2 in node.Children)
				{
					if (!this.Extract(node2, destination))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000028F4 File Offset: 0x00000AF4
		private void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.cancellationTokenSource = new CancellationTokenSource();
			this.thread = new Thread(delegate()
			{
				try
				{
					Directory.CreateDirectory(this.Destination);
					this.Extract(this.Node, this.Destination);
					base.Dispatcher.Invoke(new Action(delegate()
					{
						base.DialogResult = new bool?(true);
					}), new object[0]);
				}
				catch (Exception exception)
				{
					base.Dispatcher.Invoke(new Action(delegate()
					{
						MessageBox.Show(string.Format("Error extracting {1}:{0}{0}{2}", Environment.NewLine, Node.Name, exception.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
						DialogResult = new bool?(false);
					}), new object[0]);
				}
			})
			{
				Name = "ExtractThread",
				IsBackground = true
			};
			this.thread.Start();
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002942 File Offset: 0x00000B42
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.thread != null)
			{
				this.cancellationTokenSource.Cancel();
				this.thread.Join();
				this.cancellationTokenSource = null;
				this.thread = null;
			}
			base.DialogResult = new bool?(false);
		}

		// Token: 0x0400000F RID: 15
		private CancellationTokenSource cancellationTokenSource;

		// Token: 0x04000010 RID: 16
		private Thread thread;
	}
}
