using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ArchiveViewer.Properties
{
	// Token: 0x0200001C RID: 28
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x060000D0 RID: 208 RVA: 0x00005271 File Offset: 0x00003471
		internal Resources()
		{
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x0000527C File Offset: 0x0000347C
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("ArchiveViewer.Properties.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x000052B5 File Offset: 0x000034B5
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x000052BC File Offset: 0x000034BC
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x04000088 RID: 136
		private static ResourceManager resourceMan;

		// Token: 0x04000089 RID: 137
		private static CultureInfo resourceCulture;
	}
}
