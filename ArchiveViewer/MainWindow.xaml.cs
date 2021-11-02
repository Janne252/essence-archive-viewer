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
			Archives = new ObservableCollection<Archive>();
			Items = new ObservableCollection<INode>();
			InitializeComponent();
			windowSettingsRestorer = new WindowSettingsRestorer(this, App.Settings.MainWindow, new Size(1024.0, 704.0), new Size(0.0, 0.0));
			windowSettingsRestorer.Restore();
			currentArchive = (ObjectDataProvider)FindResource("CurrentArchive");
			currentNode = (ObjectDataProvider)FindResource("CurrentNode");
			openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Filter = "Archive File (*.sga)|*.sga";
			openFileDialog.Multiselect = true;
			saveFileDialog = new Microsoft.Win32.SaveFileDialog();
			folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.ShowNewFolderButton = true;
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
			if (!ReferenceEquals(currentArchive.ObjectInstance, node.Archive))
			{
				currentArchive.ObjectInstance = node.Archive;
				currentArchive.Refresh();
			}
			if (node.Children != null)
			{
				if (!ReferenceEquals(currentNode.ObjectInstance, node))
				{
					currentNode.ObjectInstance = node;
					currentNode.Refresh();
					return;
				}
			}
			else if (node.Parent != null && !ReferenceEquals(currentNode.ObjectInstance, node.Parent))
			{
				currentNode.ObjectInstance = node.Parent;
				currentNode.Refresh();
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x0000410C File Offset: 0x0000230C
		private void setSelectedNode(INode node, bool focus)
		{
			if (node != null)
			{
				bool flag = currentNode.ObjectInstance == null;
				setObjectInstances(node);
				if (node.Children != null || flag)
				{
					ItemsControl itemsControl = Tree;
					foreach (INode node2 in parents(node).Reverse<INode>())
					{
						TreeViewItem treeViewItem = itemsControl.ItemContainerGenerator.ContainerFromItem(node2) as TreeViewItem;
						if (treeViewItem == null)
						{
							break;
						}
						treeViewItem.IsExpanded = true;
						if (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
						{
							Tree.Dispatcher.Invoke(new Action(delegate()
							{
							}), DispatcherPriority.Render, new object[0]);
						}
						if (ReferenceEquals(node2, node))
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
			if (ReferenceEquals(source, Tree))
			{
				return Tree.SelectedItem as INode;
			}
			if (ReferenceEquals(source, List) && List.SelectedItems.Count == 1)
			{
				return List.SelectedItem as INode;
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
					Archive archive = Archives.FirstOrDefault((Archive a) => a.FullName.Equals(fullName, StringComparison.InvariantCultureIgnoreCase));
					if (archive == null)
					{
						archive = new Archive(fullName);
						Archives.Add(archive);
					}
					setSelectedNode(archive, true);
					App.Settings.AddRecentFile(fullName);
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error opening {0}:{1}{1}{2}", text, Environment.NewLine, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
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
				System.Windows.MessageBox.Show(string.Format("Error executing {1}:{0}{0}{2}", Environment.NewLine, file.Name, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004410 File Offset: 0x00002610
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			open((from arg in Environment.GetCommandLineArgs().Skip(1)
			where System.IO.File.Exists(arg)
			select arg).Distinct<string>());
		}

		// Token: 0x060000AF RID: 175 RVA: 0x0000444C File Offset: 0x0000264C
		private void File_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			RecentFiles.Items.Clear();
			RecentFiles.IsEnabled = (App.Settings.RecentFiles.Count > 0);
			foreach (string text in App.Settings.RecentFiles)
			{
				System.Windows.Controls.MenuItem menuItem = new System.Windows.Controls.MenuItem
				{
					Header = text,
					Tag = text
				};
				menuItem.Click += OpenRecent_Click;
				RecentFiles.Items.Add(menuItem);
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004500 File Offset: 0x00002700
		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (openFileDialog.ShowDialog() ?? false)
			{
				open(openFileDialog.FileNames);
			}
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004540 File Offset: 0x00002740
		private void OpenRecent_Click(object sender, RoutedEventArgs e)
		{
			string text = (string)((System.Windows.Controls.MenuItem)sender).Tag;
			if (System.IO.File.Exists(text))
			{
				open(new string[]
				{
					text
				});
				return;
			}
			if (System.Windows.MessageBox.Show(this, string.Format("{0} not found. Remove from list?", text), Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				App.Settings.RemoveRecentFile(text);
			}
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000045A1 File Offset: 0x000027A1
		private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (currentArchive.ObjectInstance != null);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000045BC File Offset: 0x000027BC
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Archive archive = currentArchive.ObjectInstance as Archive;
			if (archive != null)
			{
				Archives.Remove(archive);
				archive.Dispose();
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000045F0 File Offset: 0x000027F0
		private void CloseAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (Archives.Count > 0);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004608 File Offset: 0x00002808
		private void CloseAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Archive[] array = Archives.ToArray<Archive>();
			Archives.Clear();
			foreach (Archive archive in array)
			{
				archive.Dispose();
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004646 File Offset: 0x00002846
		private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004650 File Offset: 0x00002850
		private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (new FindWindow(options)
			{
				Owner = this
			}.ShowDialog() ?? false)
			{
				Func<INode, bool> predicate;
				try
				{
					predicate = options.GetPredicate();
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error parsing what:{0}{0}{1}", Environment.NewLine, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				Items.Clear();
				foreach (Archive node in Archives)
				{
					find(node, predicate);
				}
				currentArchive.ObjectInstance = null;
				currentArchive.Refresh();
				currentNode.ObjectInstance = null;
				currentNode.Refresh();
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004750 File Offset: 0x00002950
		private void find(INode node, Func<INode, bool> predicate)
		{
			if (predicate(node))
			{
				Items.Add(node);
			}
			if (node.Children != null)
			{
				foreach (INode node2 in node.Children)
				{
					find(node2, predicate);
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
			List.SelectAll();
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000047EC File Offset: 0x000029EC
		private void BrowseUp_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (currentNode != null)
			{
				INode node = currentNode.ObjectInstance as INode;
				if (node != null && node.Parent != null)
				{
					setSelectedNode(node.Parent, true);
				}
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0000482A File Offset: 0x00002A2A
		private void Node_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (getSelectedNode(e.Source) != null);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004844 File Offset: 0x00002A44
		private void List_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (List.SelectedItem is INode);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004860 File Offset: 0x00002A60
		private void Extract_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = getSelectedNode(e.Source);
			if (selectedNode != null)
			{
                Essence.Core.IO.Archive.File file = selectedNode as Essence.Core.IO.Archive.File;
				if (file != null)
				{
					saveFileDialog.Filter = ((!string.IsNullOrWhiteSpace(file.Extension)) ? string.Format("{0} File (*.{1})|*.{1}", file.Extension.ToUpperInvariant(), file.Extension.ToLowerInvariant()) : "All Files|*");
					saveFileDialog.FileName = ((!string.IsNullOrEmpty(extractPath)) ? Path.Combine(extractPath, selectedNode.Name) : selectedNode.Name);
					if (!(saveFileDialog.ShowDialog() ?? false))
					{
						return;
					}
					try
					{
						extractPath = Path.GetDirectoryName(saveFileDialog.FileName);
					}
					catch (Exception)
					{
					}
					try
					{
						System.IO.File.WriteAllBytes(saveFileDialog.FileName, file.GetData());
						return;
					}
					catch (Exception ex)
					{
						System.Windows.MessageBox.Show(string.Format("Error writing {1} to {2}:{0}{0}{3}", new object[]
						{
							Environment.NewLine,
							file.Name,
							saveFileDialog.FileName,
							ex.Message
						}), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
						return;
					}
				}
				folderBrowserDialog.Description = string.Format("Extract {0} to:", selectedNode.Name);
				if (!string.IsNullOrEmpty(extractPath))
				{
					folderBrowserDialog.SelectedPath = extractPath;
				}
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					extractPath = folderBrowserDialog.SelectedPath;
					new ProgressWindow(selectedNode, extractPath)
					{
						Owner = this
					}.ShowDialog();
				}
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004A3C File Offset: 0x00002C3C
		private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = getSelectedNode(e.Source);
			if (selectedNode != null)
			{
				System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, selectedNode.FullName);
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004A6C File Offset: 0x00002C6C
		private void OpenFileLocation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode node = List.SelectedItem as INode;
			if (node != null)
			{
				setSelectedNode(node, false);
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004A98 File Offset: 0x00002C98
		private void Properties_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			INode selectedNode = getSelectedNode(e.Source);
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
				setObjectInstances(node);
				if (node.Children == null)
				{
					return;
				}
				Items.Clear();
				using (IEnumerator<INode> enumerator = node.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						INode item = enumerator.Current;
						Items.Add(item);
					}
					return;
				}
			}
			currentArchive.ObjectInstance = null;
			currentArchive.Refresh();
			currentNode.ObjectInstance = null;
			currentNode.Refresh();
			Items.Clear();
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004BA0 File Offset: 0x00002DA0
		private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
                Essence.Core.IO.Archive.File file = Tree.SelectedItem as Essence.Core.IO.Archive.File;
				if (file != null)
				{
					execute(file);
				}
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004BD0 File Offset: 0x00002DD0
		private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				INode node = List.SelectedItem as INode;
				if (node != null)
				{
                    Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
					if (file != null)
					{
						execute(file);
						return;
					}
					setSelectedNode(node, false);
				}
			}
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004C14 File Offset: 0x00002E14
		private void FileDrag_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (dragStart != null && dragItem != null && e.LeftButton == MouseButtonState.Pressed)
			{
				Vector vector = dragStart.Value - e.GetPosition(null);
				if (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(dragItem);
					if (itemsControl != null)
					{
						INode node = itemsControl.ItemContainerGenerator.ItemFromContainer(dragItem) as INode;
						if (node != null)
						{
                            Essence.Core.IO.Archive.File file = node as Essence.Core.IO.Archive.File;
							if (file != null)
							{
								string text = Path.Combine(Path.GetTempPath(), file.Name);
								try
								{
									System.IO.File.WriteAllBytes(text, file.GetData());
									DragDrop.DoDragDrop(dragItem, new System.Windows.DataObject(System.Windows.DataFormats.FileDrop, new string[]
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
									}), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
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
				dragStart = new Point?(e.GetPosition(null));
				dragItem = dependencyObject;
				return;
			}
			dragStart = null;
			dragItem = null;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004DDB File Offset: 0x00002FDB
		private void FileDrag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			dragStart = null;
			dragItem = null;
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
					open(array);
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
			eventSetter.Event = PreviewMouseRightButtonDownEvent;
			eventSetter.Handler = new MouseButtonEventHandler(TreeViewItem_PreviewMouseDown);
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
