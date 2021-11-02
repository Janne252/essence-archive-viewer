using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace ArchiveViewer
{
	// Token: 0x02000002 RID: 2
	public partial class AboutWindow : Window
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public AboutWindow()
		{
			this.AssemblyName = Assembly.GetEntryAssembly().GetName();
			this.InitializeComponent();
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000206E File Offset: 0x0000026E
		// (set) Token: 0x06000003 RID: 3 RVA: 0x00002076 File Offset: 0x00000276
		public AssemblyName AssemblyName { get; private set; }

		// Token: 0x06000004 RID: 4 RVA: 0x0000207F File Offset: 0x0000027F
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
        }
    }
}
