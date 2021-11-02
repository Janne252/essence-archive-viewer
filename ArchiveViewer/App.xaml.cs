using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace ArchiveViewer
{
	// Token: 0x0200000F RID: 15
	public partial class App : Application
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000076 RID: 118 RVA: 0x0000371A File Offset: 0x0000191A
		public static Settings Settings
		{
			get
			{
				return settings;
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003721 File Offset: 0x00001921
		private void Application_Exit(object sender, ExitEventArgs e)
		{
			settings.Save();
		}

		// Token: 0x0400003A RID: 58
		private static Settings settings = Settings.Load();
	}
}
