using System;
using System.Runtime.Serialization;
using System.Windows;

namespace ArchiveViewer
{
	// Token: 0x0200000B RID: 11
	[DataContract]
	public sealed class WindowSettings
	{
		// Token: 0x0600004C RID: 76 RVA: 0x00002DDD File Offset: 0x00000FDD
		public WindowSettings()
		{
			Top = 0;
			Left = 0;
			Width = 0;
			Height = 0;
			State = WindowState.Normal;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002E08 File Offset: 0x00001008
		public WindowSettings(int top, int left, int width, int height, WindowState state)
		{
			Top = top;
			Left = left;
			Width = width;
			Height = height;
			State = state;
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600004E RID: 78 RVA: 0x00002E35 File Offset: 0x00001035
		// (set) Token: 0x0600004F RID: 79 RVA: 0x00002E3D File Offset: 0x0000103D
		[DataMember]
		public int Top { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00002E46 File Offset: 0x00001046
		// (set) Token: 0x06000051 RID: 81 RVA: 0x00002E4E File Offset: 0x0000104E
		[DataMember]
		public int Left { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000052 RID: 82 RVA: 0x00002E57 File Offset: 0x00001057
		// (set) Token: 0x06000053 RID: 83 RVA: 0x00002E5F File Offset: 0x0000105F
		[DataMember]
		public int Width { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000054 RID: 84 RVA: 0x00002E68 File Offset: 0x00001068
		// (set) Token: 0x06000055 RID: 85 RVA: 0x00002E70 File Offset: 0x00001070
		[DataMember]
		public int Height { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00002E79 File Offset: 0x00001079
		// (set) Token: 0x06000057 RID: 87 RVA: 0x00002E81 File Offset: 0x00001081
		[DataMember]
		public WindowState State { get; set; }
	}
}
