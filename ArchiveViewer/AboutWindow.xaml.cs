using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ArchiveViewer
{
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			AssemblyName = Assembly.GetEntryAssembly().GetName();
			InitializeComponent();
		}

		public AssemblyName AssemblyName { get; private set; }

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
