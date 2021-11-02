using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Essence.Core.IO.Archive;

namespace ArchiveViewer
{
	// Token: 0x02000006 RID: 6
	public partial class FindWindow : Window
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002186 File Offset: 0x00000386
		public FindWindow(FindOptions options)
		{
			Options = options;
			InitializeComponent();
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000E RID: 14 RVA: 0x0000219B File Offset: 0x0000039B
		// (set) Token: 0x0600000F RID: 15 RVA: 0x000021A3 File Offset: 0x000003A3
		public FindOptions Options { get; private set; }

		// Token: 0x06000010 RID: 16 RVA: 0x000021AC File Offset: 0x000003AC
		private void Find_Loaded(object sender, RoutedEventArgs e)
		{
			What.SelectAll();
			What.Focus();
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000021C5 File Offset: 0x000003C5
		private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !string.IsNullOrEmpty(Options.What);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000021E0 File Offset: 0x000003E0
		private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = new bool?(true);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000021EE File Offset: 0x000003EE
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = new bool?(false);
		}

		// Token: 0x02000007 RID: 7
		public class FindOptions
		{
			// Token: 0x06000016 RID: 22 RVA: 0x000022D4 File Offset: 0x000004D4
			public FindOptions()
			{
				What = string.Empty;
				MatchCase = false;
			}

			// Token: 0x17000003 RID: 3
			// (get) Token: 0x06000017 RID: 23 RVA: 0x000022EE File Offset: 0x000004EE
			// (set) Token: 0x06000018 RID: 24 RVA: 0x000022F6 File Offset: 0x000004F6
			public string What { get; set; }

			// Token: 0x17000004 RID: 4
			// (get) Token: 0x06000019 RID: 25 RVA: 0x000022FF File Offset: 0x000004FF
			// (set) Token: 0x0600001A RID: 26 RVA: 0x00002307 File Offset: 0x00000507
			public bool MatchCase { get; set; }

			// Token: 0x17000005 RID: 5
			// (get) Token: 0x0600001B RID: 27 RVA: 0x00002310 File Offset: 0x00000510
			// (set) Token: 0x0600001C RID: 28 RVA: 0x00002318 File Offset: 0x00000518
			public FindMethod Method { get; set; }

			// Token: 0x0600001D RID: 29 RVA: 0x00002384 File Offset: 0x00000584
			public Func<INode, bool> GetPredicate()
			{
				switch (Method)
				{
				case FindMethod.Wildcards:
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("^");
					int num;
					for (int i = 0; i < What.Length; i = num + 1)
					{
						num = What.IndexOfAny(new char[]
						{
							'*',
							'?'
						}, i);
						if (num == -1)
						{
							stringBuilder.Append(Regex.Escape(What.Substring(i)));
							break;
						}
						if (num > i)
						{
							stringBuilder.Append(Regex.Escape(What.Substring(i, num - i)));
						}
						char c = What[num];
						if (c != '*')
						{
							if (c == '?')
							{
								stringBuilder.Append(".");
							}
						}
						else
						{
							stringBuilder.Append(".*");
						}
					}
					stringBuilder.Append("$");
					Regex regx = new Regex(stringBuilder.ToString(), MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
					return (INode n) => regx.IsMatch(n.Name);
				}
				case FindMethod.RegularExpression:
				{
					Regex regx = new Regex(What, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
					return (INode n) => regx.IsMatch(n.Name);
				}
				default:
				{
					StringComparison stringComparison = MatchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
					return (INode n) => n.Name.IndexOf(What, stringComparison) >= 0;
				}
				}
			}
		}
	}
}
