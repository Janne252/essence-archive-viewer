using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Essence.Core.IO.Archive;

namespace ArchiveViewer
{
	// Token: 0x0200001B RID: 27
	public partial class MainWindow : Window, IStyleConnector
	{
		// Token: 0x060000A3 RID: 163 RVA: 0x00003E2C File Offset: 0x0000202C
		public MainWindow()
		{
			this.Archives = new ObservableCollection<Archive>();
			this.Items = new ObservableCollection<INode>();
			this.InitializeComponent();
			this.windowSettingsRestorer = new WindowSettingsRestorer(this, App.Settings.MainWindow, new Size(1024.0, 704.0), new Size(0.0, 0.0));
			this.windowSettingsRestorer.Restore();
			this.currentArchive = (ObjectDataProvider)base.FindResource("CurrentArchive");
			this.currentNode = (ObjectDataProvider)base.FindResource("CurrentNode");
			this.openFileDialog = new Microsoft.Win32.OpenFileDialog();
			this.openFileDialog.Filter = "Archive File (*.sga)|*.sga";
			this.openFileDialog.Multiselect = true;
			this.saveFileDialog = new Microsoft.Win32.SaveFileDialog();
			this.folderBrowserDialog = new FolderBrowserDialog();
			this.folderBrowserDialog.ShowNewFolderButton = true;
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x00003F2A File Offset: 0x0000212A
		// (set) Token: 0x060000A5 RID: 165 RVA: 0x00003F32 File Offset: 0x00002132
		public ObservableCollection<Archive> Archives { get; private set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00003F3B File Offset: 0x0000213B
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x00003F43 File Offset: 0x00002143
		public ObservableCollection<INode> Items { get; private set; }

		// Token: 0x060000A8 RID: 168 RVA: 0x00004038 File Offset: 0x00002238
		private static IEnumerable<INode> parents(INode node)
		{
			while (node != null)
			{
				yield return node;
				node = node.Parent;
			}
			yield break;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00004058 File Offset: 0x00002258
		private void setObjectInstances(INode node)
		{
			if (!object.ReferenceEquals(this.currentArchive.ObjectInstance, node.Archive))
			{
				this.currentArchive.ObjectInstance = node.Archive;
				this.currentArchive.Refresh();
			}
			if (node.Children != null)
			{
				if (!object.ReferenceEquals(this.currentNode.ObjectInstance, node))
				{
					this.currentNode.ObjectInstance = node;
					this.currentNode.Refresh();
					return;
				}
			}
			else if (node.Parent != null && !object.ReferenceEquals(this.currentNode.ObjectInstance, node.Parent))
			{
				this.currentNode.ObjectInstance = node.Parent;
				this.currentNode.Refresh();
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x0000410C File Offset: 0x0000230C
		private void setSelectedNode(INode node, bool focus)
		{
			if (node != null)
			{
				bool flag = this.currentNode.ObjectInstance == null;
				this.setObjectInstances(node);
				if (node.Children != null || flag)
				{
					ItemsControl itemsControl = this.Tree;
					foreach (INode node2 in MainWindow.parents(node).Reverse<INode>())
					{
						TreeViewItem treeViewItem = itemsControl.ItemContainerGenerator.ContainerFromItem(node2) as TreeViewItem;
						if (treeViewItem == null)
						{
							break;
						}
						treeViewItem.IsExpanded = true;
						if (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
						{
							this.Tree.Dispatcher.Invoke(new Action(delegate()
							{
							}), DispatcherPriority.Render, new object[0]);
						}
						if (object.ReferenceEquals(node2, node))
						{
							treeViewItem.IsSelected = true;
							treeViewItem.BringIntoView();
							if (focus)
							{
								treeViewItem.Focus();
							}
						}
						itemsControl = treeViewItem;
					}
				}
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004214 File Offset: 0x00002414
		private INode getSelectedNode(object source)
		{
			if (object.ReferenceEquals(source, this.Tree))
			{
				return this.Tree.SelectedItem as INode;
			}
			if (object.ReferenceEquals(source, this.List) && this.List.SelectedItems.Count == 1)
			{
				return this.List.SelectedItem as INode;
			}
			return null;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00004290 File Offset: 0x00002490
		private void open(IEnumerable<string> fileNames)
		{
			foreach (string text in fileNames)
			{
				try
				{
					string fullName = Path.GetFullPath(text);
					Archive archive = this.Archives.FirstOrDefault((Archive a) => a.FullName.Equals(fullName, StringComparison.InvariantCultureIgnoreCase));
					if (archive == null)
					{
						archive = new Archive(fullName);
						this.Archives.Add(archive);
					}
					this.setSelectedNode(archive, true);
					App.Settings.AddRecentFile(fullName);
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error opening {0}:{1}{1}{2}", text, Environment.NewLine, ex.Message), base.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004370 File Offset: 0x00002570
		private void execute(Essence.Core.IO.Archive.File file)
		{
			try
			{
				string text = Path.Combine(Path.GetTempPath(), file.Name);
				System.IO.File.WriteAllBytes(text, file.GetData());
				Process.Start(new ProcessStartInfo(text)
				{
					ErrorDialog = true,
					ErrorDialogParentHandle = new WindowInteropHelper(this).Handle,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(string.Format("Error executing {1}:{0}{0}{2}", Environment.NewLine, file.Name, ex.Message), base.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004410 File Offset: 0x00002610
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.open((from arg in Environment.GetCommandLineArgs().Skip(1)
			where System.IO.File.Exists(arg)
			select arg).Distinct<string>());
		}

		// Token: 0x060000AF RID: 175 RVA: 0x0000444C File Offset: 0x0000264C
		private void File_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			this.RecentFiles.Items.Clear();
			this.RecentFiles.IsEnabled = (App.Settings.RecentFiles.Count > 0);
			foreach (string text in App.Settings.RecentFiles)
			{
				System.Windows.Controls.MenuItem menuItem = new System.Windows.Controls.MenuItem
				{
					Header = text,
					Tag = text
				};
				menuItem.Click += this.OpenRecent_Click;
				this.RecentFiles.Items.Add(menuItem);
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004500 File Offset: 0x00002700
		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.openFileDialog.ShowDialog() ?? false)
			{
				this.open(this.openFileDialog.FileNames);
			}
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004540 File Offset: 0x00002740
		private void OpenRecent_Click(object sender, RoutedEventArgs e)
		{
			string text = (string)((System.Windows.Controls.MenuItem)sender).Tag;
			if (System.IO.File.Exists(text))
			{
				this.open(new string[]
				{
					text
				});
				return;
			}
			if (System.Windows.MessageBox.Show(this, string.Format("{0} not found. Remove from list?", text), base.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				App.Settings.RemoveRecentFile(text);
			}
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000045A1 File Offset: 0x000027A1
		private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.currentArchive.ObjectInstance != null);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000045BC File Offset: 0x000027BC
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Archive archive = this.currentArchive.ObjectInstance as Archive;
			if (archive != null)
			{
				this.Archives.Remove(archive);
				archive.Dispose();
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000045F0 File Offset: 0x000027F0
		private void CloseAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.Archives.Count > 0);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004608 File Offset: 0x00002808
		private void CloseAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Archive[] array = this.Archives.ToArray<Archive>();
			this.Archives.Clear();
			foreach (Archive archive in array)
			{
				archive.Dispose();
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004646 File Offset: 0x00002846
		private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			base.Close();
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004650 File Offset: 0x00002850
		private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (new FindWindow(this.options)
			{
				Owner = this
			}.ShowDialog() ?? false)
			{
				Func<INode, bool> predicate;
				try
				{
					predicate = this.options.GetPredicate();
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error parsing what:{0}{0}{1}", Environment.NewLine, ex.Message), base.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				this.Items.Clear();
				foreach (Archive node in this.Archives)
				{
					this.find(node, predicate);
				}
				this.currentArchive.ObjectInstance = null;
				this.currentArchive.Refresh();
				this.currentNode.ObjectInstance = null;
				this.currentNode.Refresh();
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004750 File Offset: 0x00002950
		private void find(INode node, Func<INode, bool> predicate)
		{
			if (predicate(node))
			{
				this.Items.Add(node);
			}
			if (node.Children != null)
			{
				foreach (INode node2 in node.Children)
				{
					this.find(node2, predicate);
				}
			}
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x000047BC File Offset: 0x000029BC
		private void About_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			new AboutWindow
			{
				Owner = this
			}.ShowDialog();
		}

		// Token: 0x060000BA RID: 186 RVA: 0x000047DD File Offset: 0x000029DD
		private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.List.SelectAll();
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000047EC File Offset: 0x000029EC
		private void BrowseUp_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.currentNode != null)
			{
				INode node = this.currentNode.ObjectInstance as INode;
				if (node != null && node.Parent != null)
				{
					this.setSelectedNode(node.Parent, true);
				}
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0000482A File Offset: 0x00002A2A
		private void Node_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.getSelectedNode(e.Source) != null);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004844 File Offset: 0x00002A44
		private void List_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.List.SelectedItem is INode);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004860 File Offset: 0x00002A60
		private void Extract_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = this.getSelectedNode(e.Source);
			if (selectedNode != null)
			{
                Essence.Core.IO.Archive.File file = selectedNode as Essence.Core.IO.Archive.File;
				if (file != null)
				{
					this.saveFileDialog.Filter = ((!string.IsNullOrWhiteSpace(file.Extension)) ? string.Format("{0} File (*.{1})|*.{1}", file.Extension.ToUpperInvariant(), file.Extension.ToLowerInvariant()) : "All Files|*");
					this.saveFileDialog.FileName = ((!string.IsNullOrEmpty(this.extractPath)) ? Path.Combine(this.extractPath, selectedNode.Name) : selectedNode.Name);
					if (!(this.saveFileDialog.ShowDialog() ?? false))
					{
						return;
					}
					try
					{
						this.extractPath = Path.GetDirectoryName(this.saveFileDialog.FileName);
					}
					catch (Exception)
					{
					}
					try
					{
						System.IO.File.WriteAllBytes(this.saveFileDialog.FileName, file.GetData());
						return;
					}
					catch (Exception ex)
					{
						System.Windows.MessageBox.Show(string.Format("Error writing {1} to {2}:{0}{0}{3}", new object[]
						{
							Environment.NewLine,
							file.Name,
							this.saveFileDialog.FileName,
							ex.Message
						}), base.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
						return;
					}
				}
				this.folderBrowserDialog.Description = string.Format("Extract {0} to:", selectedNode.Name);
				if (!string.IsNullOrEmpty(this.extractPath))
				{
					this.folderBrowserDialog.SelectedPath = this.extractPath;
				}
				if (this.folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.extractPath = this.folderBrowserDialog.SelectedPath;
					new ProgressWindow(selectedNode, this.extractPath)
					{
						Owner = this
					}.ShowDialog();
				}
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004A3C File Offset: 0x00002C3C
		private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = this.getSelectedNode(e.Source);
			if (selectedNode != null)
			{
				System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, selectedNode.FullName);
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004A6C File Offset: 0x00002C6C
		private void OpenFileLocation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode node = this.List.SelectedItem as INode;
			if (node != null)
			{
				this.setSelectedNode(node, false);
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004A98 File Offset: 0x00002C98
		private void Properties_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = this.getSelectedNode(e.Source);
			if (selectedNode != null)
			{
				new PropertiesWindow(selectedNode)
				{
					Owner = this
				}.ShowDialog();
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004ACC File Offset: 0x00002CCC
		private void TreeViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = sender as TreeViewItem;
			if (treeViewItem != null)
			{
				treeViewItem.IsSelected = true;
			}
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00004AEC File Offset: 0x00002CEC
		private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			INode node = e.NewValue as INode;
			if (node != null)
			{
				this.setObjectInstances(node);
				if (node.Children == null)
				{
					return;
				}
				this.Items.Clear();
				using (IEnumerator<INode> enumerator = node.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						INode item = enumerator.Current;
						this.Items.Add(item);
					}
					return;
				}
			}
			this.currentArchive.ObjectInstance = null;
			this.currentArchive.Refresh();
			this.currentNode.ObjectInstance = null;
			this.currentNode.Refresh();
			this.Items.Clear();
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004BA0 File Offset: 0x00002DA0
		private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
                Essence.Core.IO.Archive.File file = this.Tree.SelectedItem as Essence.Core.IO.Archive.File;
				if (file != null)
				{
					this.execute(file);
				}
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004BD0 File Offset: 0x00002DD0
		private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				INode node = this.List.SelectedItem as INode;
				if (node != null)
				{
                    Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
					if (file != null)
					{
						this.execute(file);
						return;
					}
					this.setSelectedNode(node, false);
				}
			}
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004C14 File Offset: 0x00002E14
		private void FileDrag_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (this.dragStart != null && this.dragItem != null && e.LeftButton == MouseButtonState.Pressed)
			{
				Vector vector = this.dragStart.Value - e.GetPosition(null);
				if (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(this.dragItem);
					if (itemsControl != null)
					{
						INode node = itemsControl.ItemContainerGenerator.ItemFromContainer(this.dragItem) as INode;
						if (node != null)
						{
                            Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
							if (file != null)
							{
								string text = Path.Combine(Path.GetTempPath(), file.Name);
								try
								{
									System.IO.File.WriteAllBytes(text, file.GetData());
									DragDrop.DoDragDrop(this.dragItem, new System.Windows.DataObject(System.Windows.DataFormats.FileDrop, new string[]
									{
										text
									}), System.Windows.DragDropEffects.Copy);
								}
								catch (Exception ex)
								{
									System.Windows.MessageBox.Show(string.Format("Error writing {1} to {2}:{0}{0}{3}", new object[]
									{
										Environment.NewLine,
										file.Name,
										text,
										ex.Message
									}), base.Title, MessageBoxButton.OK, MessageBoxImage.Hand);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004D68 File Offset: 0x00002F68
		private void FileDrag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			UIElement uielement = (UIElement)sender;
			DependencyObject dependencyObject = uielement.InputHitTest(e.GetPosition(uielement)) as DependencyObject;
			while (dependencyObject != null && !(dependencyObject is System.Windows.Controls.ListViewItem) && !(dependencyObject is TreeViewItem))
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			}
			if (dependencyObject != null)
			{
				this.dragStart = new Point?(e.GetPosition(null));
				this.dragItem = dependencyObject;
				return;
			}
			this.dragStart = null;
			this.dragItem = null;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004DDB File Offset: 0x00002FDB
		private void FileDrag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.dragStart = null;
			this.dragItem = null;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00004DF0 File Offset: 0x00002FF0
		private void FileDrop_DragOver(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				e.Effects = System.Windows.DragDropEffects.Link;
				return;
			}
			e.Effects = System.Windows.DragDropEffects.None;
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00004E14 File Offset: 0x00003014
		private void FileDrop_Drop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				string[] array = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
				if (array != null)
				{
					this.open(array);
				}
			}
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00005228 File Offset: 0x00003428
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId != 17)
			{
				return;
			}
			EventSetter eventSetter = new EventSetter();
			eventSetter.Event = UIElement.PreviewMouseRightButtonDownEvent;
			eventSetter.Handler = new MouseButtonEventHandler(this.TreeViewItem_PreviewMouseDown);
			((Style)target).Setters.Add(eventSetter);
		}

		// Token: 0x04000075 RID: 117
		private WindowSettingsRestorer windowSettingsRestorer;

		// Token: 0x04000076 RID: 118
		private ObjectDataProvider currentArchive;

		// Token: 0x04000077 RID: 119
		private ObjectDataProvider currentNode;

		// Token: 0x04000078 RID: 120
		private Microsoft.Win32.OpenFileDialog openFileDialog;

		// Token: 0x04000079 RID: 121
		private Microsoft.Win32.SaveFileDialog saveFileDialog;

		// Token: 0x0400007A RID: 122
		private FolderBrowserDialog folderBrowserDialog;

		// Token: 0x0400007B RID: 123
		private string extractPath;

		// Token: 0x0400007C RID: 124
		private FindWindow.FindOptions options = new FindWindow.FindOptions();

		// Token: 0x0400007D RID: 125
		private Point? dragStart;

		// Token: 0x0400007E RID: 126
		private DependencyObject dragItem;
	}
}
