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
using Essence.Core.IO.Archive;
using File = Essence.Core.IO.Archive.File;

namespace ArchiveViewer
{
	public partial class MainWindow : Window, IStyleConnector
	{
		public MainWindow()
		{
			Archives = new ObservableCollection<Archive>();
			Items = new ObservableCollection<INode>();
			InitializeComponent();
			_windowSettingsRestorer = new WindowSettingsRestorer(this, App.Settings.MainWindow, new Size(1024.0, 704.0), new Size(0.0, 0.0));
			_windowSettingsRestorer.Restore();
			_currentArchive = (ObjectDataProvider)FindResource("CurrentArchive");
			_currentNode = (ObjectDataProvider)FindResource("CurrentNode");
			_openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archive File (*.sga)|*.sga",
                Multiselect = true
            };
            _saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            _folderBrowserDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
        }

		public ObservableCollection<Archive> Archives { get; }

		public ObservableCollection<INode> Items { get; }

		private static IEnumerable<INode> Parents(INode node)
		{
			while (node != null)
			{
				yield return node;
				node = node.Parent;
			}
			yield break;
		}

		private void SetObjectInstances(INode node)
		{
			if (!ReferenceEquals(_currentArchive.ObjectInstance, node.Archive))
			{
				_currentArchive.ObjectInstance = node.Archive;
				_currentArchive.Refresh();
			}
			if (node.Children != null)
			{
				if (!ReferenceEquals(_currentNode.ObjectInstance, node))
				{
					_currentNode.ObjectInstance = node;
					_currentNode.Refresh();
					return;
				}
			}
			else if (node.Parent != null && !ReferenceEquals(_currentNode.ObjectInstance, node.Parent))
			{
				_currentNode.ObjectInstance = node.Parent;
				_currentNode.Refresh();
			}
		}

		private void SetSelectedNode(INode node, bool focus)
		{
			if (node != null)
			{
				var flag = _currentNode.ObjectInstance == null;
				SetObjectInstances(node);
				if (node.Children != null || flag)
				{
					ItemsControl itemsControl = Tree;
					foreach (var node2 in Parents(node).Reverse<INode>())
					{
                        if (itemsControl.ItemContainerGenerator.ContainerFromItem(node2) is not TreeViewItem treeViewItem)
						{
							break;
						}
						treeViewItem.IsExpanded = true;
						if (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
						{
							Tree.Dispatcher.Invoke(new Action(delegate
                            {
							}), DispatcherPriority.Render, Array.Empty<object>());
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

		private INode GetSelectedNode(object source)
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

		private void Open(IEnumerable<string> fileNames)
		{
			foreach (var text in fileNames)
			{
				try
				{
					var fullName = Path.GetFullPath(text);
					var archive = Archives.FirstOrDefault((Archive a) => a.FullName.Equals(fullName, StringComparison.InvariantCultureIgnoreCase));
					if (archive == null)
					{
						archive = new Archive(fullName);
						Archives.Add(archive);
					}
					SetSelectedNode(archive, true);
					App.Settings.AddRecentFile(fullName);
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error opening {0}:{1}{1}{2}", text, Environment.NewLine, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		private void Execute(File file)
		{
			try
			{
				var text = Path.Combine(Path.GetTempPath(), file.Name);
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

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Open((from arg in Environment.GetCommandLineArgs().Skip(1)
			where System.IO.File.Exists(arg)
			select arg).Distinct<string>());
		}

		private void File_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			RecentFiles.Items.Clear();
			RecentFiles.IsEnabled = (App.Settings.RecentFiles.Count > 0);
			foreach (var text in App.Settings.RecentFiles)
			{
				var menuItem = new System.Windows.Controls.MenuItem
				{
					Header = text,
					Tag = text
				};
				menuItem.Click += OpenRecent_Click;
				RecentFiles.Items.Add(menuItem);
			}
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_openFileDialog.ShowDialog() ?? false)
			{
				Open(_openFileDialog.FileNames);
			}
		}

		private void OpenRecent_Click(object sender, RoutedEventArgs e)
		{
			var text = (string)((System.Windows.Controls.MenuItem)sender).Tag;
			if (System.IO.File.Exists(text))
			{
				Open(new[]
				{
					text
				});
				return;
			}
			if (System.Windows.MessageBox.Show(this, $"{text} not found. Remove from list?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				App.Settings.RemoveRecentFile(text);
			}
		}

		private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (_currentArchive.ObjectInstance != null);
		}

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
            if (_currentArchive.ObjectInstance is Archive archive)
			{
				Archives.Remove(archive);
				archive.Dispose();
			}
		}

		private void CloseAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (Archives.Count > 0);
		}

		private void CloseAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var array = Archives.ToArray<Archive>();
			Archives.Clear();
			foreach (var archive in array)
			{
				archive.Dispose();
			}
		}

		private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (new FindWindow(_options)
			{
				Owner = this
			}.ShowDialog() ?? false)
			{
				Func<INode, bool> predicate;
				try
				{
					predicate = _options.GetPredicate();
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show(string.Format("Error parsing what:{0}{0}{1}", Environment.NewLine, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				Items.Clear();
				foreach (var node in Archives)
				{
					Find(node, predicate);
				}
				_currentArchive.ObjectInstance = null;
				_currentArchive.Refresh();
				_currentNode.ObjectInstance = null;
				_currentNode.Refresh();
			}
		}

		private void Find(INode node, Func<INode, bool> predicate)
		{
			if (predicate(node))
			{
				Items.Add(node);
			}
			if (node.Children != null)
			{
				foreach (var node2 in node.Children)
				{
					Find(node2, predicate);
				}
			}
		}

		private void About_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			new AboutWindow
			{
				Owner = this
			}.ShowDialog();
		}

		private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List.SelectAll();
		}

		private void BrowseUp_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_currentNode != null)
			{
                if (_currentNode.ObjectInstance is INode {Parent: { }} node)
				{
					SetSelectedNode(node.Parent, true);
				}
			}
		}

		private void Node_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (GetSelectedNode(e.Source) != null);
		}

		private void List_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (List.SelectedItem is INode);
		}

		private void Extract_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedNode = GetSelectedNode(e.Source);
			if (selectedNode != null)
			{
                if (selectedNode is File file)
				{
					_saveFileDialog.Filter = ((!string.IsNullOrWhiteSpace(file.Extension)) ? string.Format("{0} File (*.{1})|*.{1}", file.Extension.ToUpperInvariant(), file.Extension.ToLowerInvariant()) : "All Files|*");
					_saveFileDialog.FileName = ((!string.IsNullOrEmpty(_extractPath)) ? Path.Combine(_extractPath, selectedNode.Name) : selectedNode.Name);
					if (!(_saveFileDialog.ShowDialog() ?? false))
					{
						return;
					}
					try
					{
						_extractPath = Path.GetDirectoryName(_saveFileDialog.FileName);
					}
					catch (Exception)
					{
					}
					try
					{
						System.IO.File.WriteAllBytes(_saveFileDialog.FileName, file.GetData());
						return;
					}
					catch (Exception ex)
					{
						System.Windows.MessageBox.Show(string.Format("Error writing {1} to {2}:{0}{0}{3}", new object[]
						{
							Environment.NewLine,
							file.Name,
							_saveFileDialog.FileName,
							ex.Message
						}), Title, MessageBoxButton.OK, MessageBoxImage.Hand);
						return;
					}
				}
				_folderBrowserDialog.Description = $"Extract {selectedNode.Name} to:";
				if (!string.IsNullOrEmpty(_extractPath))
				{
					_folderBrowserDialog.SelectedPath = _extractPath;
				}
				if (_folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					_extractPath = _folderBrowserDialog.SelectedPath;
					new ProgressWindow(selectedNode, _extractPath)
					{
						Owner = this
					}.ShowDialog();
				}
			}
		}

		private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedNode = GetSelectedNode(e.Source);
			if (selectedNode != null)
			{
				System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, selectedNode.FullName);
			}
		}

		private void OpenFileLocation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
            if (List.SelectedItem is INode node)
			{
				SetSelectedNode(node, false);
			}
		}

		private void Properties_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedNode = GetSelectedNode(e.Source);
			if (selectedNode != null)
			{
				new PropertiesWindow(selectedNode)
				{
					Owner = this
				}.ShowDialog();
			}
		}

		private void TreeViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
            if (sender is TreeViewItem treeViewItem)
			{
				treeViewItem.IsSelected = true;
			}
		}

		private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
            if (e.NewValue is INode node)
			{
				SetObjectInstances(node);
				if (node.Children == null)
				{
					return;
				}
				Items.Clear();
                using var enumerator = node.Children.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    Items.Add(item);
                }
                return;
            }
			_currentArchive.ObjectInstance = null;
			_currentArchive.Refresh();
			_currentNode.ObjectInstance = null;
			_currentNode.Refresh();
			Items.Clear();
		}

		private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
                if (Tree.SelectedItem is File file)
				{
					Execute(file);
				}
			}
		}

		private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
                if (List.SelectedItem is INode node)
				{
                    if (node is File file)
					{
						Execute(file);
						return;
					}
					SetSelectedNode(node, false);
				}
			}
		}

		private void FileDrag_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (_dragStart != null && _dragItem != null && e.LeftButton == MouseButtonState.Pressed)
			{
				var vector = _dragStart.Value - e.GetPosition(null);
				if (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					var itemsControl = ItemsControl.ItemsControlFromItemContainer(_dragItem);
					if (itemsControl != null)
					{
                        if (itemsControl.ItemContainerGenerator.ItemFromContainer(_dragItem) is INode and File file)
						{
                            var text = Path.Combine(Path.GetTempPath(), file.Name);
                            try
                            {
                                System.IO.File.WriteAllBytes(text, file.GetData());
                                DragDrop.DoDragDrop(_dragItem, new System.Windows.DataObject(System.Windows.DataFormats.FileDrop, new[]
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

		private void FileDrag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var uielement = (UIElement)sender;
			var dependencyObject = uielement.InputHitTest(e.GetPosition(uielement)) as DependencyObject;
			while (dependencyObject != null && dependencyObject is not System.Windows.Controls.ListViewItem && dependencyObject is not TreeViewItem)
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			}
			if (dependencyObject != null)
			{
				_dragStart = e.GetPosition(null);
				_dragItem = dependencyObject;
				return;
			}
			_dragStart = null;
			_dragItem = null;
		}

		private void FileDrag_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_dragStart = null;
			_dragItem = null;
		}

		private void FileDrop_DragOver(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				e.Effects = System.Windows.DragDropEffects.Link;
				return;
			}
			e.Effects = System.Windows.DragDropEffects.None;
		}

		private void FileDrop_Drop(object sender, System.Windows.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
                if (e.Data.GetData(System.Windows.DataFormats.FileDrop, true) is string[] array)
				{
					Open(array);
				}
			}
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId != 17)
			{
				return;
			}
			var eventSetter = new EventSetter
            {
                Event = PreviewMouseRightButtonDownEvent,
                Handler = new MouseButtonEventHandler(TreeViewItem_PreviewMouseDown)
            };
            ((Style)target).Setters.Add(eventSetter);
		}

		private readonly WindowSettingsRestorer _windowSettingsRestorer;

		private readonly ObjectDataProvider _currentArchive;

		private readonly ObjectDataProvider _currentNode;

		private readonly Microsoft.Win32.OpenFileDialog _openFileDialog;

		private readonly Microsoft.Win32.SaveFileDialog _saveFileDialog;

		private readonly FolderBrowserDialog _folderBrowserDialog;

		private string _extractPath;

		private readonly FindWindow.FindOptions _options = new();

		private Point? _dragStart;

		private DependencyObject _dragItem;
	}
}
