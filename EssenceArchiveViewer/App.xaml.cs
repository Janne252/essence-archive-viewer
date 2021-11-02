using System.Windows;

namespace EssenceArchiveViewer
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
