using System;
using System.Windows.Input;

namespace ArchiveViewer
{
	// Token: 0x0200000D RID: 13
	public static class WindowCommands
	{
		// Token: 0x0400002C RID: 44
		public static readonly RoutedUICommand Close = new RoutedUICommand("Close", "Close", typeof(WindowCommands));

		// Token: 0x0400002D RID: 45
		public static readonly RoutedUICommand CloseAll = new RoutedUICommand("Close All", "CloseAll", typeof(WindowCommands));

		// Token: 0x0400002E RID: 46
		public static readonly RoutedUICommand NavBack = new RoutedUICommand("Back", "NavBack", typeof(WindowCommands));

		// Token: 0x0400002F RID: 47
		public static readonly RoutedUICommand NavForward = new RoutedUICommand("Forward", "NavForward", typeof(WindowCommands));

		// Token: 0x04000030 RID: 48
		public static readonly RoutedUICommand NavUp = new RoutedUICommand("Up", "NavUp", typeof(WindowCommands));

		// Token: 0x04000031 RID: 49
		public static readonly RoutedUICommand Extract = new RoutedUICommand("Extract", "Extract", typeof(WindowCommands));

		// Token: 0x04000032 RID: 50
		public static readonly RoutedUICommand CopyPath = new RoutedUICommand("Copy Path", "CopyPath", typeof(WindowCommands));

		// Token: 0x04000033 RID: 51
		public static readonly RoutedUICommand OpenFileLocation = new RoutedUICommand("Open File Location", "OpenFileLocation", typeof(WindowCommands));

		// Token: 0x04000034 RID: 52
		public static readonly RoutedUICommand Properties = new RoutedUICommand("Properties", "Properties", typeof(WindowCommands));
	}
}
