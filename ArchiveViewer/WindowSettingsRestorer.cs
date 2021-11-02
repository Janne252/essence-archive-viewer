using System;
using System.Windows;

namespace ArchiveViewer
{
	// Token: 0x0200000E RID: 14
	internal class WindowSettingsRestorer
	{
		// Token: 0x06000068 RID: 104 RVA: 0x000034A4 File Offset: 0x000016A4
		public WindowSettingsRestorer(Window window, WindowSettings settings, Size defaultSize, Size minimumSize)
		{
			Window = window;
			Settings = settings;
			DefaultSize = defaultSize;
			MinimumSize = minimumSize;
			Window.LocationChanged += delegate(object s, EventArgs e)
			{
				if (!restoring)
				{
					Rect restoreBounds = Window.RestoreBounds;
					Settings.Left = (int)restoreBounds.Left;
					Settings.Top = (int)restoreBounds.Top;
				}
			};
			Window.SizeChanged += delegate(object s, SizeChangedEventArgs e)
			{
				if (!restoring)
				{
					Rect restoreBounds = Window.RestoreBounds;
					Settings.Height = (int)restoreBounds.Height;
					Settings.Width = (int)restoreBounds.Width;
				}
			};
			Window.StateChanged += delegate(object s, EventArgs e)
			{
				if (!restoring && Window.WindowState != WindowState.Minimized)
				{
					Settings.State = Window.WindowState;
				}
			};
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000069 RID: 105 RVA: 0x0000352E File Offset: 0x0000172E
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00003536 File Offset: 0x00001736
		public Window Window { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600006B RID: 107 RVA: 0x0000353F File Offset: 0x0000173F
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00003547 File Offset: 0x00001747
		public WindowSettings Settings { get; private set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00003550 File Offset: 0x00001750
		// (set) Token: 0x0600006E RID: 110 RVA: 0x00003558 File Offset: 0x00001758
		public Size DefaultSize { get; private set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003561 File Offset: 0x00001761
		// (set) Token: 0x06000070 RID: 112 RVA: 0x00003569 File Offset: 0x00001769
		public Size MinimumSize { get; private set; }

		// Token: 0x06000071 RID: 113 RVA: 0x00003574 File Offset: 0x00001774
		public void Restore()
		{
			restoring = true;
			double virtualScreenWidth = SystemParameters.VirtualScreenWidth;
			double virtualScreenHeight = SystemParameters.VirtualScreenHeight;
			if (Settings.Width <= 0 && Settings.Height <= 0)
			{
				Window.Width = Math.Max(MinimumSize.Width, Math.Min(DefaultSize.Width, virtualScreenWidth));
				Window.Height = Math.Max(MinimumSize.Height, Math.Min(DefaultSize.Height, virtualScreenHeight));
			}
			else
			{
				Window.Left = Math.Min((double)Settings.Left, virtualScreenWidth - 32.0);
				Window.Top = Math.Min((double)Settings.Top, virtualScreenHeight - 32.0);
				Window.Width = Math.Max(MinimumSize.Width, Math.Min((double)Settings.Width, virtualScreenWidth));
				Window.Height = Math.Max(MinimumSize.Height, Math.Min((double)Settings.Height, virtualScreenHeight));
			}
			switch (Settings.State)
			{
			case WindowState.Normal:
				Window.WindowState = WindowState.Normal;
				break;
			case WindowState.Maximized:
				Window.WindowState = WindowState.Maximized;
				break;
			}
			restoring = false;
		}

		// Token: 0x04000035 RID: 53
		private bool restoring;
	}
}
