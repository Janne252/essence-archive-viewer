using System.Windows;

namespace ArchiveViewer
{
	public partial class App : Application
	{
		public static Settings Settings => settings;

        private void Application_Exit(object sender, ExitEventArgs e)
		{
			settings.Save();
		}

		private static readonly Settings settings = Settings.Load();
	}
}
