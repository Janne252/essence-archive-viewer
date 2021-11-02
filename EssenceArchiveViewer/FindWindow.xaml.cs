using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Essence.Core.IO.Archive;

namespace EssenceArchiveViewer
{
	public partial class FindWindow : Window
	{
		public FindWindow(FindOptions options)
		{
			Options = options;
			InitializeComponent();
		}

		public FindOptions Options { get; private set; }

		private void Find_Loaded(object sender, RoutedEventArgs e)
		{
			What.SelectAll();
			What.Focus();
		}

		private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !string.IsNullOrEmpty(Options.What);
		}

		private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = false;
		}

		public class FindOptions
		{
			public FindOptions()
			{
				What = string.Empty;
				MatchCase = false;
			}
			
			public string What { get; set; }
			
			public bool MatchCase { get; set; }
			
			public FindMethod Method { get; set; }
			
			public Func<INode, bool> GetPredicate()
			{
				switch (Method)
				{
				case FindMethod.Wildcards:
				{
					var stringBuilder = new StringBuilder();
					stringBuilder.Append("^");
					int num;
					for (var i = 0; i < What.Length; i = num + 1)
					{
						num = What.IndexOfAny(new[]
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
						var c = What[num];
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
					var regx = new Regex(stringBuilder.ToString(), MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
					return (INode n) => regx.IsMatch(n.Name);
				}
				case FindMethod.RegularExpression:
				{
					var regx = new Regex(What, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
					return (INode n) => regx.IsMatch(n.Name);
				}
				default:
				{
					var stringComparison = MatchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
					return (INode n) => n.Name.IndexOf(What, stringComparison) >= 0;
				}
				}
			}
		}
	}
}
