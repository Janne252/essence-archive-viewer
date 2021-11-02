using System;
using System.Windows;

namespace ArchiveViewer
{
    internal class WindowSettingsRestorer
	{
        public WindowSettingsRestorer(Window window, WindowSettings settings, Size defaultSize, Size minimumSize)
		{
			Window = window;
			Settings = settings;
			DefaultSize = defaultSize;
			MinimumSize = minimumSize;
			Window.LocationChanged += delegate
            {
				if (!restoring)
				{
					var restoreBounds = Window.RestoreBounds;
					Settings.Left = (int)restoreBounds.Left;
					Settings.Top = (int)restoreBounds.Top;
				}
			};
			Window.SizeChanged += delegate
            {
				if (!restoring)
				{
					var restoreBounds = Window.RestoreBounds;
					Settings.Height = (int)restoreBounds.Height;
					Settings.Width = (int)restoreBounds.Width;
				}
			};
			Window.StateChanged += delegate
            {
				if (!restoring && Window.WindowState != WindowState.Minimized)
				{
					Settings.State = Window.WindowState;
				}
			};
		}

		public Window Window { get; }

		public WindowSettings Settings { get; }

		public Size DefaultSize { get; }

		public Size MinimumSize { get; }

		public void Restore()
		{
			restoring = true;
			var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
			var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
			if (Settings.Width <= 0 && Settings.Height <= 0)
			{
				Window.Width = Math.Max(MinimumSize.Width, Math.Min(DefaultSize.Width, virtualScreenWidth));
				Window.Height = Math.Max(MinimumSize.Height, Math.Min(DefaultSize.Height, virtualScreenHeight));
			}
			else
			{
				Window.Left = Math.Min(Settings.Left, virtualScreenWidth - 32.0);
				Window.Top = Math.Min(Settings.Top, virtualScreenHeight - 32.0);
				Window.Width = Math.Max(MinimumSize.Width, Math.Min(Settings.Width, virtualScreenWidth));
				Window.Height = Math.Max(MinimumSize.Height, Math.Min(Settings.Height, virtualScreenHeight));
			}

            Window.WindowState = Settings.State switch
            {
                WindowState.Normal => WindowState.Normal,
                WindowState.Maximized => WindowState.Maximized,
                _ => Window.WindowState
            };
            restoring = false;
		}

		private bool restoring;
	}
}
