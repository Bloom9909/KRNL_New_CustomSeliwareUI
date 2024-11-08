using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Markup;

namespace KrnlUI.Controls;

public partial class CommunityEntry : UserControl, IComponentConnector
{
	public List<string> Tags = new List<string>();

	public string Script { get; set; }

	public CommunityEntry()
	{
		InitializeComponent();
	}
}
