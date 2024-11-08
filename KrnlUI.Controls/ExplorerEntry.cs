using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace KrnlUI.Controls;

public partial class ExplorerEntry : UserControl, IComponentConnector
{
	public bool isFile = true;

	private MainWindow window { get; set; }

	public string Script { get; set; }

	public string Path { get; set; }

	public ExplorerEntry(MainWindow window)
	{
		InitializeComponent();
		this.window = window;
		Script = "";
	}

	public void SetLuaTheme()
	{
		FolderIcon.Visibility = Visibility.Hidden;
		FolderIcon2.Visibility = Visibility.Hidden;
		LuaIcon.Visibility = Visibility.Visible;
		LuaIcon2.Visibility = Visibility.Visible;
		BorderBack.Background = new SolidColorBrush(Color.FromRgb(24, 160, 251));
		isFile = true;
	}

	public void SetFolderTheme()
	{
		FolderIcon.Visibility = Visibility.Visible;
		FolderIcon2.Visibility = Visibility.Visible;
		LuaIcon.Visibility = Visibility.Hidden;
		LuaIcon2.Visibility = Visibility.Hidden;
		BorderBack.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));
		isFile = false;
	}

	private void Grid_MouseEnter(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Hand;
	}

	private void Grid_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Arrow;
	}

	private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		window.FileContextMenu.Visibility = Visibility.Hidden;
		Select();
	}

	public void Select()
	{
		if (window.SelectedExpEntry != null)
		{
			window.SelectedExpEntry.CardBorderDown.To = Color.FromArgb(17, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			window.SelectedExpEntry.CardBorderUp.To = Color.FromArgb(17, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			window.SelectedExpEntry.CardBorderDownSB.Begin();
		}
		CardBorderDown.To = Color.FromRgb(24, 160, 251);
		CardBorderUp.To = Color.FromRgb(24, 160, 251);
		CardBorderDownSB.Begin();
		window.SelectedExpEntry = this;
	}

	private void EntryEdit_LostFocus(object sender, RoutedEventArgs e)
	{
		if (EntryEdit.Text == "")
		{
			EntryName.Visibility = Visibility.Visible;
			EntryEdit.Visibility = Visibility.Hidden;
			return;
		}
		EntryName.Content = EntryEdit.Text;
		EntryName.Visibility = Visibility.Visible;
		EntryEdit.Visibility = Visibility.Hidden;
		string text = System.IO.Path.GetDirectoryName(Path) + $"\\{EntryName.Content}";
		if (isFile)
		{
			if (!File.Exists(text))
			{
				File.Move(Path, text);
			}
		}
		else if (!Directory.Exists(text))
		{
			Directory.Move(Path, text);
		}
		Path = text;
	}
}
