using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace KrnlUI.Controls;

public partial class Tab : UserControl, IComponentConnector
{
	public bool isSelected;

	public bool Cancelled;

	public string Script = "\"\"";

	public string Path = "";

	public bool isWatched;

	public bool watchSave;

	public bool isDeleted;

	public int DefaultedNameTabNr;

	private int passWidth;

	public bool isSaved = true;

	public bool IsSaved
	{
		get
		{
			return isSaved;
		}
		set
		{
			if (value && !isSaved)
			{
				if (TabName[0] == '*')
				{
					TabName = TabName.Remove(0, 1);
				}
			}
			else if (isSaved)
			{
				TabName = "*" + TabName;
			}
			isSaved = value;
		}
	}

	public string TabName
	{
		get
		{
			string text = TabTitle.Content.ToString();
			if (text[0] == '*')
			{
				text = text.Remove(0, 1);
			}
			return text;
		}
		set
		{
			isSaved = false;
			IsSaved = true;
			FormattedText formattedText = new FormattedText(value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(TabTitle.FontFamily, TabTitle.FontStyle, TabTitle.FontWeight, TabTitle.FontStretch), TabTitle.FontSize, Brushes.Black, new NumberSubstitution(), 1.0);
			passWidth = 78;
			if (formattedText.Width > 42.0)
			{
				if (formattedText.Width < 122.0 || base.Width < 122.0)
				{
					int num = (int)formattedText.Width - 42;
					passWidth = 78 + num;
					TabTitle.Width += num;
				}
				else
				{
					passWidth = 122;
					TabTitle.Width += 122.0;
				}
			}
			Storyboard storyboard = new Storyboard();
			DoubleAnimation doubleAnimation = new DoubleAnimation();
			storyboard.Children.Add(doubleAnimation);
			doubleAnimation.From = base.Width;
			doubleAnimation.To = passWidth;
			doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100));
			doubleAnimation.DecelerationRatio = 0.4;
			Storyboard.SetTarget(doubleAnimation, this);
			Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
			storyboard.Begin();
			if (value.StartsWith("Untitled ") && value.Length > 9 && int.TryParse(value.Remove(0, 9), out var result))
			{
				DefaultedNameTabNr = result;
			}
			TabTitle.Content = value;
		}
	}

	public new StackPanel Parent { get; set; }

	public MainWindow MainInstance { get; set; }

	public FileWatcher fileWatcher { get; set; }

	public Tab(StackPanel Parent, MainWindow Instance)
	{
		InitializeComponent();
		this.Parent = Parent;
		MainInstance = Instance;
		fileWatcher = new FileWatcher(this);
	}

	public void Select()
	{
		isSelected = true;
		if (MainInstance.MenuDown)
		{
			MainInstance.LastTab = this;
		}
		MainInstance.BringUpMenu(isTabCaller: true);
		if (Common.SelectedTab != null)
		{
			if (Common.SelectedTab != this)
			{
				Common.SelectedTab.TabTitle.Visibility = Visibility.Visible;
				Common.SelectedTab.EntryEditName.Visibility = Visibility.Hidden;
			}
			Common.SelectedTab.Script = MainInstance.ReadScript().ToString();
			Common.SelectedTab.Deselect();
		}
		MainInstance.WriteScript(Script, tabPrompt: true);
		Common.SelectedTab = this;
		Storyboard storyboard = new Storyboard();
		ColorAnimation colorAnimation = new ColorAnimation();
		storyboard.Children.Add(colorAnimation);
		colorAnimation.From = Color.FromRgb(122, 122, 122);
		colorAnimation.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		colorAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100));
		colorAnimation.DecelerationRatio = 0.4;
		Storyboard.SetTarget(colorAnimation, TabTitle);
		Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Foreground.Color"));
		MouseEnterBackground.To = Color.FromRgb(44, 44, 44);
		MouseLeaveBackground.To = Color.FromRgb(44, 44, 44);
		MouseEnterBackgroundAnim.Begin();
		storyboard.Begin();
	}

	public void Deselect()
	{
		isSelected = false;
		Common.SelectedTab = null;
		Common.PreviousTab = this;
		Storyboard storyboard = new Storyboard();
		ColorAnimation colorAnimation = new ColorAnimation();
		storyboard.Children.Add(colorAnimation);
		colorAnimation.From = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		colorAnimation.To = Color.FromRgb(122, 122, 122);
		colorAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100));
		colorAnimation.DecelerationRatio = 0.4;
		Storyboard.SetTarget(colorAnimation, TabTitle);
		Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Foreground.Color"));
		MouseEnterBackground.To = Color.FromRgb(39, 39, 39);
		MouseLeaveBackground.To = Color.FromRgb(34, 34, 34);
		MouseLeaveBackgroundAnim.Begin();
		storyboard.Begin();
	}

	private void MainTab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Point position = e.GetPosition(svg434);
		if (!(position.X >= 0.0) || !(position.X < 24.0) || !(position.Y >= 0.0) || !(position.Y < 24.0))
		{
			Select();
		}
	}

	public bool PromptSaveDialog(Tab tab)
	{
		tab.Cancelled = false;
		if (tab.TabTitle.Content.ToString().IndexOf('*') == 0 && tab.TabName != tab.TabTitle.Content.ToString())
		{
			switch (MessageBox.Show("Do you want to save the changes you made to " + tab.TabName + "?\n\nYour changes will be lost if you don't save them.", "KRNL", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation))
			{
			case MessageBoxResult.Cancel:
				tab.Cancelled = true;
				return false;
			case MessageBoxResult.No:
				return true;
			default:
				MainInstance.HBOpts.Visibility = Visibility.Hidden;
				MainInstance.FileOpts.Visibility = Visibility.Hidden;
				if (tab.Path == "" || !File.Exists(tab.Path))
				{
					SaveFileDialog saveFileDialog = new SaveFileDialog();
					saveFileDialog.DefaultExt = "lua";
					saveFileDialog.Filter = "Lua files (*.lua)|*.lua";
					if (saveFileDialog.ShowDialog() == true)
					{
						tab.TabName = saveFileDialog.SafeFileName;
						tab.Path = saveFileDialog.FileName;
						File.WriteAllText(saveFileDialog.FileName, tab.Script);
						tab.isSaved = false;
						tab.IsSaved = true;
					}
				}
				else
				{
					File.WriteAllText(tab.Path, tab.Script);
				}
				return true;
			}
		}
		return true;
	}

	public void svg434_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (!PromptSaveDialog(this))
		{
			if (e != null)
			{
				e.Handled = true;
			}
			return;
		}
		isDeleted = true;
		if (Common.SelectedTab == this)
		{
			Script = MainInstance.ReadScript().ToString();
		}
		if (e != null && Common.SelectedTab == this)
		{
			int num = Parent.Children.IndexOf(this);
			if (Parent.Children.Count > 2)
			{
				if (num > 0)
				{
					((Tab)Parent.Children[num - 1]).Select();
				}
				else if (Parent.Children.Count - num > 2)
				{
					((Tab)Parent.Children[num + 1]).Select();
				}
			}
		}
		if (Common.SelectedTab == this)
		{
			MainInstance.SaveRecent(TabName, MainInstance.ReadScript());
		}
		else
		{
			MainInstance.SaveRecent(TabName, Script);
		}
		MainInstance.TabFlow.Children.Remove(this);
		if (MainInstance.TabFlow.Children.Count <= 1)
		{
			MainInstance.BringDownMenu();
		}
	}

	private void TabCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		EntryEditName.Focus();
		EntryEditName.SelectAll();
		Keyboard.Focus(EntryEditName);
		TabTitle.Visibility = Visibility.Hidden;
		EntryEditName.Visibility = Visibility.Visible;
		EntryEditName.Text = TabName;
	}

	private void EntryEditName_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			TabTitle.Visibility = Visibility.Visible;
			EntryEditName.Visibility = Visibility.Hidden;
			TabName = "*" + EntryEditName.Text;
		}
		else if (e.Key == Key.Escape)
		{
			TabTitle.Visibility = Visibility.Visible;
			EntryEditName.Visibility = Visibility.Hidden;
		}
	}

	private void EntryEditName_LostFocus(object sender, RoutedEventArgs e)
	{
		TabTitle.Visibility = Visibility.Visible;
		EntryEditName.Visibility = Visibility.Hidden;
	}

	private void MainTab_GotFocus(object sender, RoutedEventArgs e)
	{
		if (EntryEditName.Visibility == Visibility.Visible)
		{
			EntryEditName.Focus();
		}
	}
}
