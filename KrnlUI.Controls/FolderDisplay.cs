using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace KrnlUI.Controls;

public partial class FolderDisplay : UserControl, IComponentConnector
{
	public string Path = "";

	private bool isFileRaw;

	public bool isFile
	{
		get
		{
			return isFileRaw;
		}
		set
		{
			isFileRaw = value;
			if (value)
			{
				svg240.Visibility = Visibility.Collapsed;
			}
			else
			{
				svg240.Visibility = Visibility.Visible;
			}
		}
	}

	public FolderDisplay()
	{
		InitializeComponent();
	}
}
