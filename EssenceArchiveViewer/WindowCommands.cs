using System.Windows.Input;

namespace EssenceArchiveViewer
{
    public static class WindowCommands
	{
        public static readonly RoutedUICommand Close = new("Close", "Close", typeof(WindowCommands));
        public static readonly RoutedUICommand CloseAll = new("Close All", "CloseAll", typeof(WindowCommands));
        public static readonly RoutedUICommand NavBack = new("Back", "NavBack", typeof(WindowCommands));
        public static readonly RoutedUICommand NavForward = new("Forward", "NavForward", typeof(WindowCommands));
        public static readonly RoutedUICommand NavUp = new("Up", "NavUp", typeof(WindowCommands));
        public static readonly RoutedUICommand Extract = new("Extract", "Extract", typeof(WindowCommands));
        public static readonly RoutedUICommand CopyPath = new("Copy Path", "CopyPath", typeof(WindowCommands));
        public static readonly RoutedUICommand OpenFileLocation = new("Open File Location", "OpenFileLocation", typeof(WindowCommands));
        public static readonly RoutedUICommand Properties = new("Properties", "Properties", typeof(WindowCommands));
	}
}
