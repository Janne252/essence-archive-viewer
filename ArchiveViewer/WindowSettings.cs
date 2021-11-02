using System.Runtime.Serialization;
using System.Windows;

namespace ArchiveViewer
{
    [DataContract]
	public sealed class WindowSettings
	{
        public WindowSettings()
		{
			Top = 0;
			Left = 0;
			Width = 0;
			Height = 0;
			State = WindowState.Normal;
		}

		public WindowSettings(int top, int left, int width, int height, WindowState state)
		{
			Top = top;
			Left = left;
			Width = width;
			Height = height;
			State = state;
		}

		[DataMember]
		public int Top { get; set; }

		[DataMember]
		public int Left { get; set; }

		[DataMember]
		public int Width { get; set; }

		[DataMember]
		public int Height { get; set; }

		[DataMember]
		public WindowState State { get; set; }
	}
}
