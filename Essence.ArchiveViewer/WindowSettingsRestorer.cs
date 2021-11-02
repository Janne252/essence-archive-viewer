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
			this.Window = window;
			this.Settings = settings;
			this.DefaultSize = defaultSize;
			this.MinimumSize = minimumSize;
			this.Window.LocationChanged += delegate(object s, EventArgs e)
			{
				if (!this.restoring)
				{
					Rect restoreBounds = this.Window.RestoreBounds;
					this.Settings.Left = (int)restoreBounds.Left;
					this.Settings.Top = (int)restoreBounds.Top;
				}
			};
			this.Window.SizeChanged += delegate(object s, SizeChangedEventArgs e)
			{
				if (!this.restoring)
				{
					Rect restoreBounds = this.Window.RestoreBounds;
					this.Settings.Height = (int)restoreBounds.Height;
					this.Settings.Width = (int)restoreBounds.Width;
				}
			};
			this.Window.StateChanged += delegate(object s, EventArgs e)
			{
				if (!this.restoring && this.Window.WindowState != WindowState.Minimized)
				{
					this.Settings.State = this.Window.WindowState;
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
			this.restoring = true;
			double virtualScreenWidth = SystemParameters.VirtualScreenWidth;
			double virtualScreenHeight = SystemParameters.VirtualScreenHeight;
			if (this.Settings.Width <= 0 && this.Settings.Height <= 0)
			{
				this.Window.Width = Math.Max(this.MinimumSize.Width, Math.Min(this.DefaultSize.Width, virtualScreenWidth));
				this.Window.Height = Math.Max(this.MinimumSize.Height, Math.Min(this.DefaultSize.Height, virtualScreenHeight));
			}
			else
			{
				this.Window.Left = Math.Min((double)this.Settings.Left, virtualScreenWidth - 32.0);
				this.Window.Top = Math.Min((double)this.Settings.Top, virtualScreenHeight - 32.0);
				this.Window.Width = Math.Max(this.MinimumSize.Width, Math.Min((double)this.Settings.Width, virtualScreenWidth));
				this.Window.Height = Math.Max(this.MinimumSize.Height, Math.Min((double)this.Settings.Height, virtualScreenHeight));
			}
			switch (this.Settings.State)
			{
			case WindowState.Normal:
				this.Window.WindowState = WindowState.Normal;
				break;
			case WindowState.Maximized:
				this.Window.WindowState = WindowState.Maximized;
				break;
			}
			this.restoring = false;
		}

		// Token: 0x04000035 RID: 53
		private bool restoring;
	}
}
