using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using IWshRuntimeLibrary;
using KrnlUI.Controls;
using Microsoft.Win32;
using SeliwareAPI;

namespace KrnlUI;

public partial class MainWindow : Window, IComponentConnector
{
	private List<CommunityEntry> CommunityCards = new List<CommunityEntry>();

	private bool TabChanging;

	private bool DragAvailable = true;

	private bool AutoAttachEnabled;

	private bool isAutoAttached;

	private bool AutoLaunchEnabled = true;

	private int TabCount = 2;

	public bool MenuDown;

	private bool tabs_saving;

	private int FileHBOptOpen;

	private int EditHBOptOpen;

	private List<(string, string, string)> Scripts = new List<(string, string, string)> { ("OpenGui", "loadstring(game:HttpGet('https://pastebin.com/raw/UXmbai5q', true))()", "stickmasterluke") };

	private string CurrentDraftPath = "Scripts";

	private bool isAnimatingNotf;

	private int PrefHBOptOpen;

	private int ViewHBOptOpen;

	private Mutex auto_launch_mutex;

	private bool Searchable = true;

	public ChromiumWebBrowser browser { get; set; }

	public static Dispatcher dispatcher { get; set; }

	private Grid selectedTab { get; set; }

	public Tab LastTab { get; set; }

	public ExplorerEntry SelectedExpEntry { get; set; }

	private UIElement FileContextSelected { get; set; }

	private string roblox_path { get; set; } = "";


	public void WriteScript(string script, bool tabPrompt)
	{
		TabChanging = tabPrompt;
		if (((FrameworkElement)(object)browser).IsLoaded)
		{
			WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "SetText", new object[1] { script });
		}
	}

	public string ReadScript()
	{
		if (((FrameworkElement)(object)browser).IsLoaded)
		{
			string text = WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "(function() { return GetText() })();", (TimeSpan?)null, false).GetAwaiter().GetResult()
				.Result.ToString();
			if (text != null)
			{
				return text;
			}
			return "";
		}
		return "";
	}

	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Exception ex = (Exception)e.ExceptionObject;
		File.WriteAllText("error.txt", string.Join("Message: " + ex.Message, "StackTrace: " + ex.StackTrace));
    }

	public MainWindow()
	{
		Seliware.Initialize();
        if (Process.GetProcessesByName("KrnlUI").Length > 1)
		{
			Environment.Exit(0);
			return;
		}
		dispatcher = base.Dispatcher;
		//Console.Title = "Krnl UWP Console";
		ConsoleFramework.Init();
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		string text = "Krnl Workspace";
		while (true)
		{
			int num;
			if ((num = Framework.krnl_dll.IsUpdated()) != 1)
			{
				switch (num)
				{
				case -1:
					break;
				case -3:
					MessageBox.Show("Krnl is not updated for the current Fuck version!\nMake sure you update ROBLOX!", "Krnl", MessageBoxButton.OK, MessageBoxImage.Hand);
					Environment.Exit(0);
					return;
				default:
				{
					try
					{
						if (Directory.Exists("workspace"))
						{
							Directory.Move("workspace", "old_workspace");
						}
						if (Directory.Exists("autoexec"))
						{
							Directory.Move("autoexec", "old_autoexec");
						}
					}
					catch
					{
					}
					string text3 = Path.Combine(text, "workspace");
					string text4 = Path.Combine(text, "autoexec");
					string path = Path.Combine(text, "ipc");
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(text3);
					}
					if (!Directory.Exists(text4))
					{
						Directory.CreateDirectory(text4);
					}
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					if (!File.Exists("workspace.lnk"))
					{
						WshShell wshShell = (WshShell)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
						IWshShortcut obj2 = (IWshShortcut)(dynamic)wshShell.CreateShortcut("workspace.lnk");
						obj2.TargetPath = text3;
						obj2.Save();
					}
					if (!File.Exists("autoexec.lnk"))
					{
						WshShell wshShell2 = (WshShell)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
						IWshShortcut obj3 = (IWshShortcut)(dynamic)wshShell2.CreateShortcut("autoexec.lnk");
						obj3.TargetPath = text4;
						obj3.Save();
					}
					InitializeComponent();
					MainMenu.Visibility = Visibility.Hidden;
					InitBrowser();
					Initialize();
					InitHotkeys();
					LoadCommunity();
					MainDirDisplay.isFile = true;
					MainDirDisplay.Path = "Scripts";
					MainMenu.Margin = new Thickness(0.0, 37.0, 0.0, 0.0);
					InitRecents();
					ActivateRecent();
					AutoAttach();
					ConsoleFramework.TailFrom(Path.Combine(path, "k_ipc.txt"));
					RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", writable: true);
					if (registryKey.OpenSubKey("Krnl") == null)
					{
						RegistryKey registryKey2 = registryKey.CreateSubKey("Krnl", writable: true);
						registryKey2.SetValue("AutoAttach", false);
						registryKey2.SetValue("AutoLaunch", false);
						registryKey2.SetValue("Topmost", false);
						registryKey2.SetValue("UnlockFPS", false);
						registryKey2.SetValue("EditorMinimap", false);
					}
					RegistryKey krnlSubkey = getKrnlSubkey();
					object value = krnlSubkey.GetValue("Topmost");
					object value2 = krnlSubkey.GetValue("AutoAttach");
					object value3 = krnlSubkey.GetValue("UnlockFPS");
					object value4 = krnlSubkey.GetValue("EditorMinimap");
					if (value != null && value.ToString() == "true")
					{
						TopmostOpt_MouseLeftButtonUp(null, null);
					}
					if (value2 != null && value2.ToString() == "true")
					{
						AutoAttachOpt_MouseLeftButtonUp(null, null);
					}
					if (value3 != null && value3.ToString() == "true")
					{
						UnlockFPSOpt_MouseLeftButtonUp(null, null);
					}
					if (value4 != null && value4.ToString() == "true")
					{
						MinimapOpt_MouseLeftButtonUp(null, null);
					}
					Process[] processesByName = Process.GetProcessesByName("KrnlUI");
					foreach (Process process in processesByName)
					{
						if (Process.GetCurrentProcess().Id != process.Id)
						{
							process.Kill();
						}
					}
					BringDownMenu();
					MenuDown = true;
					return;
				}
				}
			}
			Framework.krnl_dll.Download();
        }
	}

	private void InitTabs()
	{
		if (!Directory.Exists("Data"))
		{
			Directory.CreateDirectory("Data");
			return;
		}
		if (!Directory.Exists("Data\\SavedTabs"))
		{
			Directory.CreateDirectory("Data\\SavedTabs");
			return;
		}
		if (File.Exists("Data\\SavedTabs\\tabs.config"))
		{
			string[] array = File.ReadAllLines("Data\\SavedTabs\\tabs.config");
			foreach (string text in array)
			{
				string text2 = "Data\\SavedTabs\\" + text;
				if (Directory.Exists(text2))
				{
					CreateTab(Path.GetFileName(text2), File.ReadAllText(text2 + "\\script.lua"), File.ReadAllText(text2 + "\\tab.config"));
					Directory.Delete(text2, recursive: true);
				}
			}
			File.Delete("Data\\SavedTabs\\tabs.config");
		}
		if (TabFlow.Children.Count == 1)
		{
			CreateTab("Untitled", "");
		}
	}

	private void InitHotkeys()
	{
		RoutedCommand routedCommand = new RoutedCommand();
		routedCommand.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
		RoutedCommand routedCommand2 = new RoutedCommand();
		routedCommand2.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
		RoutedCommand routedCommand3 = new RoutedCommand();
		routedCommand3.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
		RoutedCommand routedCommand4 = new RoutedCommand();
		routedCommand4.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
		base.CommandBindings.Add(new CommandBinding(routedCommand, CreateTabHotkey));
		base.CommandBindings.Add(new CommandBinding(routedCommand2, OpenHotkey));
		base.CommandBindings.Add(new CommandBinding(routedCommand3, SaveHotkey));
		base.CommandBindings.Add(new CommandBinding(routedCommand4, SaveAsHotkey));
	}

	private void CreateTabHotkey(object sender, ExecutedRoutedEventArgs e)
	{
		CreateTab("", "").Select();
	}

	private void OpenHotkey(object sender, ExecutedRoutedEventArgs e)
	{
		PromptOpenFile();
	}

	private void SaveHotkey(object sender, ExecutedRoutedEventArgs e)
	{
		PromptSaveFile();
	}

	private void SaveAsHotkey(object sender, ExecutedRoutedEventArgs e)
	{
		PromptSaveAsFile();
	}

	private async void AutoAttach()
	{
		while (true)
		{
			if (AutoAttachEnabled && Process.GetProcessesByName("RobloxPlayerBeta").Length != 0 && !Seliware.IsInjected())
			{
			   DisplayNotification("Successfully injected into Roblox!");
			}
			await Task.Delay(300);
		}
	}

	private void LoadCommunity()
	{
		if (!Directory.Exists("Community"))
		{
			return;
		}
		string[] directories = Directory.GetDirectories("Community");
		foreach (string text in directories)
		{
			string text2 = AppDomain.CurrentDomain.BaseDirectory + "\\" + text;
			CommunityEntry communityEntry = new CommunityEntry();
			communityEntry.EntryPreview.Source = new BitmapImage(new Uri(text2 + "\\preview.png"));
			communityEntry.EntryCreatorIcon.ImageSource = new BitmapImage(new Uri(text2 + "\\profile.png"));
			communityEntry.EntryName.Content = text.Split('\\')[1];
			communityEntry.EntryCreator.Content = File.ReadAllText(text2 + "\\card.config");
			communityEntry.Script = File.ReadAllText(text2 + "\\script.lua");
			if (File.Exists(text2 + "\\tags.config"))
			{
				communityEntry.Tags = File.ReadAllText(text2 + "\\tags.config").Split(',').ToList();
			}
			communityEntry.MouseRightButtonUp += CardHolder_MouseRightButtonUp;
			communityEntry.RunBorder.MouseLeftButtonUp += CommunityCard_MouseLeftButtonUp;
			communityEntry.Width = 242.0;
			communityEntry.Height = 185.0;
			CommunityCards.Add(communityEntry);
		}
	}

	private void Initialize()
	{
		if (!Directory.Exists("Scripts"))
		{
			Directory.CreateDirectory("Scripts");
		}
		if (!Directory.Exists("Recent"))
		{
			Directory.CreateDirectory("Recent");
		}
		else
		{
			new DirectoryInfo("Recent").Delete(recursive: true);
			Directory.CreateDirectory("Recent");
		}
		Introduct.Content = "Welcome " + Environment.UserName + "!";
	}

	private void InitBrowser()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		CefSettings val = new CefSettings();
		((CefSettingsBase)val).SetOffScreenRenderingBestPerformanceArgs();
		((CefSettingsBase)val).MultiThreadedMessageLoop = true;
		((CefSettingsBase)val).DisableGpuAcceleration();
		((CefSettingsBase)val).LogSeverity = (LogSeverity)99;
		Cef.Initialize((CefSettingsBase)val);
		string currentDirectory = Directory.GetCurrentDirectory();
		browser = new ChromiumWebBrowser(currentDirectory + "\\Monaco\\Monaco.html");
		browser.ConsoleMessage += Browser_ConsoleMessage;
		browser.BrowserSettings.WindowlessFrameRate = 144;
		browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
		browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;
		((UIElement)(object)browser).AllowDrop = false;
		browser.BrowserSettings.JavascriptDomPaste = (CefState)1;
		browser.BrowserSettings.JavascriptAccessClipboard = (CefState)1;
		Editor.Children.Add((UIElement)(object)browser);
	}

	private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
	{
	}

	private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
	{
		if (!(e.Message is string))
		{
			return;
		}
		string obj = e.Message as string;
		if (obj == "save_tabs")
		{
			base.Dispatcher.Invoke(SaveTabs);
		}
		if (obj == "init")
		{
			base.Dispatcher.Invoke(InitTabs);
		}
		if (!(obj == "keydown"))
		{
			return;
		}
		base.Dispatcher.Invoke(delegate
		{
			if (Common.SelectedTab != null)
			{
				Common.SelectedTab.IsSaved = false;
			}
		});
	}

	private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
	}

	private void Button_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void ExitButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Hide();
		SaveTabs();
		disable_auto_launch();
		Cef.Shutdown();
		Environment.Exit(1);
	}

	private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		base.WindowState = WindowState.Minimized;
	}

	private void MaximizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (base.WindowState == WindowState.Maximized)
		{
			base.WindowState = WindowState.Normal;
			svg356.Visibility = Visibility.Visible;
			svg223.Visibility = Visibility.Hidden;
			base.BorderThickness = new Thickness(0.0);
		}
		else
		{
			base.WindowState = WindowState.Maximized;
			svg356.Visibility = Visibility.Hidden;
			svg223.Visibility = Visibility.Visible;
			base.BorderThickness = new Thickness(7.0);
		}
	}

	private void NewTabButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		CreateTab("", "").Select();
		MyScrollViewer.ScrollToRightEnd();
	}

	private string DynamicTabName()
	{
		string text = "Untitled ";
		List<int> list = new List<int>();
		foreach (UIElement child in TabFlow.Children)
		{
			if (child is Tab tab && tab.TabName.StartsWith(text))
			{
				list.Add(tab.DefaultedNameTabNr);
			}
		}
		list.Sort();
		return text + Enumerable.Range(1, list.Count + 1).ToList().Except(list)
			.ToList()[0];
	}

	private Tab CreateTab(string Name, string Content, string path = "")
	{
		DragAvailable = false;
		Tab tab = new Tab(TabFlow, this);
		tab.TabName = ((Name == "") ? DynamicTabName() : Name);
		tab.Script = Content;
		tab.Path = path;
		TabFlow.Children.Insert(TabFlow.Children.Count - 1, tab);
		MyScrollViewer.ScrollToRightEnd();
		DragAvailable = true;
		return tab;
	}

	private void Tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Grid grid = (Grid)sender;
		if (selectedTab != grid)
		{
			grid.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));
			((Label)grid.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue));
			selectedTab.Background = new SolidColorBrush(Color.FromRgb(34, 34, 34));
			((Label)selectedTab.Children[0]).Foreground = new SolidColorBrush(Color.FromRgb(122, 122, 122));
			selectedTab = grid;
		}
	}

	private void KrnlWindow_MouseDown(object sender, MouseButtonEventArgs e)
	{
		Point position = Mouse.GetPosition(ManagedWrapper);
		if (!(FileContextMenu.Margin.Left < position.X) || !(FileContextMenu.Margin.Left + FileContextMenu.Width > position.X) || !(FileContextMenu.Margin.Top < position.Y) || !(FileContextMenu.Margin.Top + FileContextMenu.Height > position.Y))
		{
			FileContextMenu.Visibility = Visibility.Hidden;
		}
	}

	private void appIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
	}

	private void appIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (MenuDown)
		{
			BringUpMenu(isTabCaller: false);
			MenuDown = false;
		}
		else
		{
			BringDownMenu();
			MenuDown = true;
		}
	}

	public void BringDownMenu()
	{
		LastTab = Common.SelectedTab;
		if (Common.SelectedTab != null)
		{
			Common.SelectedTab.Script = ReadScript();
			Common.SelectedTab.Deselect();
		}
		MainMenu.Visibility = Visibility.Visible;
		appIconAnimDown.To = Color.FromRgb(48, 48, 48);
		appIconAnimUp.To = Color.FromRgb(44, 44, 44);
		appIconAnimEnterBack.To = Color.FromRgb(44, 44, 44);
		appIconAnimLeaveBack.To = Color.FromRgb(44, 44, 44);
		BeginStoryboard(appIconAnimDownSB);
		BeginStoryboard(appIconAnimUpSB);
		BeginStoryboard(appIconAnimEnterBackSB);
		BeginStoryboard(appIconAnimLeaveBackSB);
		if (MainDirDisplay.EntryName.Content != "Community")
		{
			if (MainDirDisplay.Name == "MainDirDisplay")
			{
				CurrentDraftPath = "Scripts";
				LayDirPath("");
				MainDirDisplay.isFile = true;
				if (MainDirDisplay.EntryName.Content == "Scripts")
				{
					InitDrafts(MainDirDisplay.Path);
				}
				else if (MainDirDisplay.EntryName.Content == "Recent")
				{
					InitRecents();
				}
			}
			else
			{
				CurrentDraftPath = MainDirDisplay.Path;
				LayDirPath(MainDirDisplay.Path);
				InitDrafts(MainDirDisplay.Path);
			}
		}
		MenuDown = true;
	}

	public void SaveTabs()
	{
		if (tabs_saving)
		{
			return;
		}
		tabs_saving = true;
		if (!Directory.Exists("Data"))
		{
			Directory.CreateDirectory("Data");
		}
		if (Directory.Exists("Data\\SavedTabs"))
		{
			try
			{
				Directory.Delete("Data\\SavedTabs", recursive: true);
			}
			catch
			{
				FileAPI.MoveToRecycleBin("Data\\SavedTabs");
			}
		}
		Directory.CreateDirectory("Data\\SavedTabs");
		List<string> list = new List<string>();
		for (int i = 0; i < TabFlow.Children.Count - 1; i++)
		{
			Tab tab = (Tab)TabFlow.Children[i];
			Directory.CreateDirectory("Data\\SavedTabs\\" + tab.TabName);
			File.WriteAllText("Data\\SavedTabs\\" + tab.TabName + "\\tab.config", tab.Path);
			File.WriteAllText("Data\\SavedTabs\\" + tab.TabName + "\\script.lua", tab.Script);
			list.Add(tab.TabName);
		}
		File.WriteAllText("Data\\SavedTabs\\tabs.config", string.Join("\n", list));
		if (Common.SelectedTab != null)
		{
			Directory.CreateDirectory("Data\\SavedTabs\\" + Common.SelectedTab.TabName);
			File.WriteAllText("Data\\SavedTabs\\" + Common.SelectedTab.TabName + "\\tab.config", Common.SelectedTab.Path);
			File.WriteAllText("Data\\SavedTabs\\" + Common.SelectedTab.TabName + "\\script.lua", ReadScript());
		}
		tabs_saving = false;
	}

	public void BringUpMenu(bool isTabCaller)
	{
		if (TabFlow.Children.Count <= 1)
		{
			return;
		}
		if (!isTabCaller)
		{
			if (Common.PreviousTab != null)
			{
				Common.PreviousTab.Select();
			}
			else
			{
				((Tab)TabFlow.Children[0]).Select();
			}
		}
		MainMenu.Visibility = Visibility.Hidden;
		appIconAnimDown.To = Color.FromRgb(48, 48, 48);
		appIconAnimUp.To = Color.FromRgb(39, 39, 39);
		appIconAnimEnterBack.To = Color.FromRgb(39, 39, 39);
		appIconAnimLeaveBack.To = Color.FromRgb(34, 34, 34);
		BeginStoryboard(appIconAnimDownSB);
		BeginStoryboard(appIconAnimUpSB);
		BeginStoryboard(appIconAnimEnterBackSB);
		BeginStoryboard(appIconAnimLeaveBackSB);
		MenuDown = false;
	}

	private void appIcon_MouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void appIcon_MouseLeave(object sender, MouseEventArgs e)
	{
	}

	private void MovableForm_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (DragAvailable)
		{
			DragMove();
		}
	}

	private void injectOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Inject();
	}

	private void Inject()
	{
		if (Process.GetProcessesByName("RobloxPlayerBeta").Any())
		{
			Seliware.Inject();
		}
		else
		{
			DisplayNotification("Roblox is not running");
		}

	}

	private void AnimateInjecting()
	{
		LoadBar.Visibility = Visibility.Visible;
		LoaderAnim.To = Color.FromArgb(byte.MaxValue, 77, 146, byte.MaxValue);
		LoaderAnimStoryboard.Begin();
	}

	private async void AnimateInjected()
	{
		LoaderAnim.To = Color.FromArgb(0, 77, byte.MaxValue, 146);
		LoaderAnimStoryboard.Begin();
		await Task.Delay(200);
		LoadBar.Visibility = Visibility.Hidden;
	}

	private void executeOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Seliware.Execute(ReadScript());
	}

	private void IsSeliwareInjected(string script)
	{
        bool isInjected = Seliware.IsInjected();
		{
			if (true == isInjected)
			{
				MessageBox.Show("Seliware Injected");
			}
			else
			{
                DisplayNotification("Please inject before executing!");
            }

		}
		return;
	}

	private void menuOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (HBOpts.Visibility == Visibility.Hidden)
		{
			HBOpts.Visibility = Visibility.Visible;
		}
		else
		{
			HBOpts.Visibility = Visibility.Hidden;
		}
	}

	private void FileHBOpt_MouseEnter(object sender, MouseEventArgs e)
	{
		FileOpts.Visibility = Visibility.Visible;
		EditOpts.Visibility = Visibility.Hidden;
		PrefOpts.Visibility = Visibility.Hidden;
		ViewOpts.Visibility = Visibility.Hidden;
		FileHBOptOpen = 0;
	}

	private void FileHBOpt_MouseLeave(object sender, MouseEventArgs e)
	{
		if (FileHBOptOpen != 1)
		{
			FileOpts.Visibility = Visibility.Hidden;
		}
	}

	private void FileHBOptsGate_MouseEnter(object sender, MouseEventArgs e)
	{
		if (FileHBOptOpen == 0)
		{
			FileHBOptOpen = 1;
		}
	}

	private void FileOpts_MouseEnter(object sender, MouseEventArgs e)
	{
		FileHBOptOpen = 2;
	}

	private void FileOpts_MouseLeave(object sender, MouseEventArgs e)
	{
		FileHBOptOpen = 0;
		FileOpts.Visibility = Visibility.Hidden;
	}

	private void EditHBOpt_MouseEnter(object sender, MouseEventArgs e)
	{
		EditOpts.Visibility = Visibility.Visible;
		FileOpts.Visibility = Visibility.Hidden;
		PrefOpts.Visibility = Visibility.Hidden;
		ViewOpts.Visibility = Visibility.Hidden;
		EditHBOptOpen = 0;
	}

	private void EditHBOpt_MouseLeave(object sender, MouseEventArgs e)
	{
		if (EditHBOptOpen != 1)
		{
			EditOpts.Visibility = Visibility.Hidden;
		}
	}

	private void EditHBOptGate_MouseEnter(object sender, MouseEventArgs e)
	{
		if (EditHBOptOpen == 0)
		{
			EditHBOptOpen = 1;
		}
	}

	private void EditOpts_MouseEnter(object sender, MouseEventArgs e)
	{
		EditHBOptOpen = 2;
	}

	private void EditOpts_MouseLeave(object sender, MouseEventArgs e)
	{
		EditHBOptOpen = 0;
		EditOpts.Visibility = Visibility.Hidden;
	}

	private void CloseHBOpt_MouseUp(object sender, MouseButtonEventArgs e)
	{
		Process[] processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
		if (processesByName.Length != 0)
		{
			Process[] array = processesByName;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Kill();
			}
		}
		HBOpts.Visibility = Visibility.Hidden;
	}

	private void ActivateRecent()
	{
		RecentTab1.To = Color.FromRgb(24, 160, 251);
		RecentTab2.To = Color.FromRgb(24, 160, 251);
		RecentTab3.To = Color.FromRgb(24, 160, 251);
		RecentTab4.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		RecentTab5.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		RecentTab6.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		RecentTab7.To = Color.FromRgb(24, 160, 251);
		RecentTab8.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		RecentTab9.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		RecentTab10.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}

	private void DeactivateRecent()
	{
		RecentTab1.To = Color.FromRgb(38, 38, 38);
		RecentTab2.To = Color.FromRgb(34, 34, 34);
		RecentTab3.To = Color.FromRgb(24, 160, 251);
		RecentTab4.To = Color.FromRgb(125, 125, 125);
		RecentTab5.To = Color.FromRgb(125, 125, 125);
		RecentTab6.To = Color.FromRgb(125, 125, 125);
		RecentTab7.To = Color.FromRgb(38, 38, 38);
		RecentTab8.To = Color.FromRgb(125, 125, 125);
		RecentTab9.To = Color.FromRgb(125, 125, 125);
		RecentTab10.To = Color.FromRgb(125, 125, 125);
		RecentTabLabel.Foreground = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		circle19.Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path21.Stroke = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		BeginStoryboard(RecentTab2SB);
	}

	private void ActivateDrafts()
	{
		DraftsTab1.To = Color.FromRgb(24, 160, 251);
		DraftsTab2.To = Color.FromRgb(24, 160, 251);
		DraftsTab3.To = Color.FromRgb(24, 160, 251);
		DraftsTab4.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab5.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab6.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab7.To = Color.FromRgb(24, 160, 251);
		DraftsTab8.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab9.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab10.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		DraftsTab12.To = Color.FromRgb(24, 160, 251);
		DraftsTab13.To = Color.FromRgb(24, 160, 251);
		DraftsTab14.To = Color.FromRgb(24, 160, 251);
		DraftsTab15.To = Color.FromRgb(24, 160, 251);
	}

	private void DeactivateDrafts()
	{
		DraftsTab1.To = Color.FromRgb(38, 38, 38);
		DraftsTab2.To = Color.FromRgb(34, 34, 34);
		DraftsTab3.To = Color.FromRgb(24, 160, 251);
		DraftsTab4.To = Color.FromRgb(125, 125, 125);
		DraftsTab5.To = Color.FromRgb(125, 125, 125);
		DraftsTab6.To = Color.FromRgb(125, 125, 125);
		DraftsTab7.To = Color.FromRgb(38, 38, 38);
		DraftsTab8.To = Color.FromRgb(125, 125, 125);
		DraftsTab9.To = Color.FromRgb(125, 125, 125);
		DraftsTab10.To = Color.FromRgb(125, 125, 125);
		DraftsTab12.To = Color.FromRgb(38, 38, 38);
		DraftsTab13.To = Color.FromRgb(34, 34, 34);
		DraftsTab14.To = Color.FromRgb(24, 160, 251);
		DraftsTab15.To = Color.FromRgb(38, 38, 38);
		DraftsTabLabel.Foreground = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path208.Stroke = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path56.Stroke = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		BeginStoryboard(DraftsTab2SB);
	}

	private void ActivateCommunity()
	{
		CommunityTab1.To = Color.FromRgb(24, 160, 251);
		CommunityTab2.To = Color.FromRgb(24, 160, 251);
		CommunityTab3.To = Color.FromRgb(24, 160, 251);
		CommunityTab4.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		CommunityTab5.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		CommunityTab6.To = Color.FromRgb(24, 160, 251);
		CommunityTab7.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		CommunityTab8.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}

	private void DeactivateCommunity()
	{
		CommunityTab1.To = Color.FromRgb(38, 38, 38);
		CommunityTab2.To = Color.FromRgb(34, 34, 34);
		CommunityTab3.To = Color.FromRgb(24, 160, 251);
		CommunityTab4.To = Color.FromRgb(125, 125, 125);
		CommunityTab5.To = Color.FromRgb(125, 125, 125);
		CommunityTab6.To = Color.FromRgb(38, 38, 38);
		CommunityTab7.To = Color.FromRgb(125, 125, 125);
		CommunityTab8.To = Color.FromRgb(125, 125, 125);
		CommunityTabLabel.Foreground = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path20.Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		BeginStoryboard(CommunityTab2SB);
	}

	private void ActivatePlan()
	{
		UpgradeTab1.To = Color.FromRgb(24, 160, 251);
		UpgradeTab2.To = Color.FromRgb(24, 160, 251);
		UpgradeTab3.To = Color.FromRgb(24, 160, 251);
		UpgradeTab4.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		UpgradeTab5.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		UpgradeTab6.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		UpgradeTab7.To = Color.FromRgb(24, 160, 251);
		UpgradeTab8.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		UpgradeTab9.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		UpgradeTab10.To = Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}

	private void DeactivatePlan()
	{
		UpgradeTab1.To = Color.FromRgb(38, 38, 38);
		UpgradeTab2.To = Color.FromRgb(34, 34, 34);
		UpgradeTab3.To = Color.FromRgb(24, 160, 251);
		UpgradeTab4.To = Color.FromRgb(125, 125, 125);
		UpgradeTab5.To = Color.FromRgb(125, 125, 125);
		UpgradeTab6.To = Color.FromRgb(125, 125, 125);
		UpgradeTab7.To = Color.FromRgb(38, 38, 38);
		UpgradeTab8.To = Color.FromRgb(125, 125, 125);
		UpgradeTab9.To = Color.FromRgb(125, 125, 125);
		UpgradeTab10.To = Color.FromRgb(125, 125, 125);
		UpgradeTabLabel.Foreground = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path42.Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		path44.Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125));
		BeginStoryboard(UpgradeTab2SB);
	}

	private void RecentTab_MouseDown(object sender, MouseButtonEventArgs e)
	{
		DisplayRecent();
	}

	private void DisplayRecent()
	{
		Keyboard.ClearFocus();
		SearchInput.Text = "Search";
		FolderDisplayer.Children.RemoveRange(1, FolderDisplayer.Children.Count);
		MainDirDisplay.isFile = true;
		InitRecents();
		ActivateRecent();
		DeactivateDrafts();
		DeactivateCommunity();
		DeactivatePlan();
	}

	private void DraftsTab_MouseDown(object sender, MouseButtonEventArgs e)
	{
		DisplayDrafts();
	}

	private void DisplayDrafts()
	{
		Keyboard.ClearFocus();
		SearchInput.Text = "Search";
		FolderDisplayer.Children.RemoveRange(1, FolderDisplayer.Children.Count);
		MainDirDisplay.isFile = true;
		InitDrafts("Scripts");
		DeactivateRecent();
		ActivateDrafts();
		DeactivateCommunity();
		DeactivatePlan();
	}

	public void SaveRecent(string Name, string Content)
	{
		File.WriteAllText("Recent\\" + Name, Content);
	}

	private void InitRecents()
	{
		CardHolder.Children.Clear();
		MainDirDisplay.EntryName.Content = "Recent";
		string[] files = Directory.GetFiles("Recent");
		foreach (string path in files)
		{
			string script = File.ReadAllText(path);
			string fileName = Path.GetFileName(path);
			TimeSpan creationDate = DateTime.Now - File.GetLastWriteTime(path);
			CreateExplorerCard(fileName, script, creationDate, path);
		}
	}

	private void InitDrafts(string Dire)
	{
		CardHolder.Children.Clear();
		MainDirDisplay.EntryName.Content = "Scripts";
		string[] files = Directory.GetFiles(Dire);
		string[] directories = Directory.GetDirectories(Dire);
		foreach (string path in directories)
		{
			string fileName = Path.GetFileName(path);
			TimeSpan creationDate = DateTime.Now - File.GetLastWriteTime(path);
			CreateExplorerCard(fileName, "", creationDate, path).SetFolderTheme();
		}
		directories = files;
		foreach (string path2 in directories)
		{
			string fileName2 = Path.GetFileName(path2);
			if (fileName2 != "temp.bin")
			{
				string script = File.ReadAllText(path2);
				TimeSpan creationDate2 = DateTime.Now - File.GetLastWriteTime(path2);
				CreateExplorerCard(fileName2, script, creationDate2, path2).SetLuaTheme();
			}
		}
	}

	private void InitCommunity()
	{
		MainDirDisplay.EntryName.Content = "Community";
		CardHolder.Children.Clear();
		foreach (CommunityEntry communityCard in CommunityCards)
		{
			CardHolder.Children.Add(communityCard);
		}
	}

	private void CommunityTab_MouseDown(object sender, MouseButtonEventArgs e)
	{
		Keyboard.ClearFocus();
		SearchInput.Text = "Search";
		FolderDisplayer.Children.RemoveRange(1, FolderDisplayer.Children.Count);
		MainDirDisplay.isFile = true;
		InitCommunity();
		DeactivateRecent();
		DeactivateDrafts();
		ActivateCommunity();
		DeactivatePlan();
	}

	private CommunityEntry CreateCommunityCard(string Title, string Script, string CreatorName)
	{
		CommunityEntry communityEntry = new CommunityEntry();
		communityEntry.EntryName.Content = Title;
		communityEntry.Script = Script;
		communityEntry.EntryCreator.Content = CreatorName;
		communityEntry.MouseRightButtonUp += CardHolder_MouseRightButtonUp;
		communityEntry.Width = 242.0;
		communityEntry.Height = 185.0;
		CardHolder.Children.Add(communityEntry);
		communityEntry.RunBorder.MouseLeftButtonUp += CommunityCard_MouseLeftButtonUp;
		return communityEntry;
	}

	private void CommunityCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
        Seliware.Execute(ReadScript());
    }

	private ExplorerEntry CreateExplorerCard(string Title, string Script, TimeSpan CreationDate, string Path)
	{
		ExplorerEntry explorerEntry = new ExplorerEntry(this);
		explorerEntry.EntryEdit.PreviewKeyDown += Entry_PreviewKeyDown;
		explorerEntry.EntryName.Content = Title;
		explorerEntry.Script = Script;
		explorerEntry.EntryEditstamp.Content = TranslateDate(CreationDate);
		explorerEntry.Path = Path;
		explorerEntry.MouseDoubleClick += CardHolder_MouseDoubleClick;
		explorerEntry.MouseRightButtonUp += CardHolder_MouseRightButtonUp;
		explorerEntry.Width = 242.0;
		explorerEntry.Height = 185.0;
		CardHolder.Children.Add(explorerEntry);
		return explorerEntry;
	}

	private string TranslateDate(TimeSpan Date)
	{
		if (Date.Days > 0)
		{
			if (Date.Days != 1)
			{
				return $"Edited {Date.Days} days ago";
			}
			return $"Edited {Date.Days} day ago";
		}
		if (Date.Hours > 0)
		{
			if (Date.Hours != 1)
			{
				return $"Edited {Date.Hours} hours ago";
			}
			return $"Edited {Date.Hours} hour ago";
		}
		if (Date.Minutes > 0)
		{
			if (Date.Minutes != 1)
			{
				return $"Edited {Date.Minutes} minutes ago";
			}
			return $"Edited {Date.Minutes} minute ago";
		}
		if (Date.Seconds > 0)
		{
			if (Date.Seconds != 1)
			{
				return $"Edited {Date.Seconds} seconds ago";
			}
			return $"Edited {Date.Seconds} second ago";
		}
		return "Edited now";
	}

	private void AddRecent(string Name, string Content)
	{
		if (!File.Exists("Recent\\" + Name))
		{
			File.WriteAllText("Recent\\" + Name, Content);
		}
	}

	private void CardHolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton != 0)
		{
			return;
		}
		ExplorerEntry explorerEntry = (ExplorerEntry)sender;
		if (explorerEntry.isFile)
		{
			if (File.Exists(explorerEntry.Path))
			{
				explorerEntry.Script = File.ReadAllText(explorerEntry.Path);
			}
			AddRecent(explorerEntry.EntryName.Content.ToString(), explorerEntry.Script);
			Tab tab = CreateTab(explorerEntry.EntryName.Content.ToString(), explorerEntry.Script);
			tab.Path = explorerEntry.Path;
			BringUpMenu(isTabCaller: false);
			tab.Select();
		}
		else if (Directory.Exists(explorerEntry.Path))
		{
			FolderDisplayer.Children.RemoveRange(1, FolderDisplayer.Children.Count);
			MainDirDisplay.isFile = false;
			CurrentDraftPath = explorerEntry.Path;
			LayDirPath(explorerEntry.Path);
			InitDrafts(explorerEntry.Path);
		}
	}

	private void LayDirPath(string RelativePath)
	{
		string[] array = RelativePath.Split('\\');
		string text = "Drafts\\";
		for (int i = 1; i < array.Length - 1; i++)
		{
			FolderDisplay folderDisplay = new FolderDisplay();
			folderDisplay.EntryName.Content = array[i];
			folderDisplay.EntryName.Foreground = new SolidColorBrush(Color.FromRgb(163, 163, 163));
			folderDisplay.isFile = false;
			folderDisplay.MouseLeftButtonUp += FolderDisplay_MouseLeftButtonUp;
			folderDisplay.MouseEnter += FolderDisplay_MouseEnter;
			folderDisplay.MouseLeave += FolderDisplay_MouseLeave;
			text = (folderDisplay.Path = text + array[i]);
			FolderDisplayer.Children.Add(folderDisplay);
		}
		FolderDisplay folderDisplay2 = new FolderDisplay();
		folderDisplay2.EntryName.Content = array[array.Length - 1];
		folderDisplay2.EntryName.Foreground = new SolidColorBrush(Color.FromRgb(163, 163, 163));
		folderDisplay2.isFile = true;
		folderDisplay2.Path = RelativePath;
		folderDisplay2.MouseLeftButtonUp += FolderDisplay_MouseLeftButtonUp;
		folderDisplay2.MouseEnter += FolderDisplay_MouseEnter;
		folderDisplay2.MouseLeave += FolderDisplay_MouseLeave;
		FolderDisplayer.Children.Add(folderDisplay2);
		CardHolder.Children.Clear();
	}

	private void FolderDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		FolderDisplayer.Children.RemoveRange(1, FolderDisplayer.Children.Count);
		FolderDisplay folderDisplay = (FolderDisplay)sender;
		if (folderDisplay.EntryName.Content == "Community")
		{
			return;
		}
		if (folderDisplay.Name == "MainDirDisplay")
		{
			CurrentDraftPath = "Scripts";
			LayDirPath("");
			MainDirDisplay.isFile = true;
			if (folderDisplay.EntryName.Content == "Scripts")
			{
				InitDrafts(folderDisplay.Path);
			}
			else if (folderDisplay.EntryName.Content == "Recent")
			{
				InitRecents();
			}
		}
		else
		{
			CurrentDraftPath = folderDisplay.Path;
			LayDirPath(folderDisplay.Path);
			InitDrafts(folderDisplay.Path);
		}
	}

	private void FolderDisplay_MouseEnter(object sender, MouseEventArgs e)
	{
		Mouse.OverrideCursor = Cursors.Hand;
	}

	private void FolderDisplay_MouseLeave(object sender, MouseEventArgs e)
	{
		Mouse.OverrideCursor = Cursors.Arrow;
	}

	private void CardHolder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		Point position = Mouse.GetPosition(ManagedWrapper);
		FileContextMenu.Margin = new Thickness(position.X, position.Y, FileContextMenu.Margin.Right, FileContextMenu.Margin.Bottom);
		if (sender is ExplorerEntry)
		{
			FileContextMenu.Height = 157.0;
		}
		else if (sender is CommunityEntry)
		{
			FileContextMenu.Height = 41.0;
		}
		FileContextMenu.Visibility = Visibility.Visible;
		FileContextSelected = (UIElement)sender;
	}

	private void UpgradeTab_MouseDown(object sender, MouseButtonEventArgs e)
	{
		DeactivateRecent();
		DeactivateDrafts();
		DeactivateCommunity();
		ActivatePlan();
	}

	private void MyScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer scrollViewer = sender as ScrollViewer;
		if (e.Delta > 0)
		{
			scrollViewer.LineLeft();
		}
		else
		{
			scrollViewer.LineRight();
		}
		e.Handled = true;
	}

	private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void SearchGlass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Keyboard.ClearFocus();
		SearchInput.Focus();
		Keyboard.Focus(SearchInput);
		SearchInput.SelectAll();
	}

	private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (!base.IsLoaded || !Searchable)
		{
			return;
		}
		if (MainDirDisplay.EntryName.Content == "Scripts")
		{
			InitDrafts("Scripts");
		}
		else if (MainDirDisplay.EntryName.Content == "Recent")
		{
			InitRecents();
		}
		else if (MainDirDisplay.EntryName.Content == "Community")
		{
			InitCommunity();
		}
		if (!(SearchInput.Text != ""))
		{
			return;
		}
		Console.WriteLine(SearchInput.Text);
		List<UIElement> list = new List<UIElement>();
		if (MainDirDisplay.EntryName.Content == "Recent" || MainDirDisplay.EntryName.Content == "Scripts")
		{
			foreach (ExplorerEntry child in CardHolder.Children)
			{
				if (CultureInfo.CurrentUICulture.CompareInfo.IndexOf(child.EntryName.Content.ToString(), SearchInput.Text, CompareOptions.IgnoreCase) >= 0)
				{
					list.Add(child);
				}
			}
		}
		else if (MainDirDisplay.EntryName.Content == "Community")
		{
			foreach (CommunityEntry child2 in CardHolder.Children)
			{
				if (child2.EntryName.Content.ToString().ToLower().IndexOf(SearchInput.Text.ToLower()) >= 0)
				{
					list.Add(child2);
				}
				else
				{
					if (child2.Tags.Count <= 0)
					{
						continue;
					}
					for (int i = 0; i < child2.Tags.Count; i++)
					{
						if (child2.Tags[i].ToString().ToLower().StartsWith(SearchInput.Text.ToLower()))
						{
							list.Add(child2);
							break;
						}
					}
				}
			}
		}
		CardHolder.Children.Clear();
		foreach (UIElement item in list)
		{
			CardHolder.Children.Add(item);
		}
	}

	private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
	{
		SearchInput.Clear();
	}

	private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
	{
		Activate();
	}

	private void OpenFileOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		PromptOpenFile();
	}

	private void PromptOpenFile()
	{
		HBOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.DefaultExt = "lua";
		if (openFileDialog.ShowDialog() == true)
		{
			string text = File.ReadAllText(openFileDialog.FileName);
			Tab tab = CreateTab(openFileDialog.SafeFileName, text);
			tab.Path = openFileDialog.FileName;
			tab.fileWatcher.filePath = tab.Path;
			tab.fileWatcher.Start();
			tab.Select();
			WriteScript(text, tabPrompt: false);
		}
	}

	private void SaveFileOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		PromptSaveAsFile();
	}

	private void PromptSaveAsFile()
	{
		HBOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
		if (Common.SelectedTab != null)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Scripts";
			saveFileDialog.FileName = Common.SelectedTab.TabName;
			saveFileDialog.DefaultExt = "lua";
			saveFileDialog.Filter = "Lua files (*.lua)|*.lua";
			if (saveFileDialog.ShowDialog() == true)
			{
				File.WriteAllText(saveFileDialog.FileName, ReadScript());
				Common.SelectedTab.IsSaved = true;
				Common.SelectedTab.Path = saveFileDialog.FileName;
				Common.SelectedTab.fileWatcher.filePath = saveFileDialog.FileName;
				Common.SelectedTab.fileWatcher.Start();
			}
		}
	}

	private async void DisplayNotification(string Text)
	{
		if (!isAnimatingNotf)
		{
			FileNotfText.Content = Text;
			FileExecNotf.Visibility = Visibility.Visible;
			Storyboard sb = new Storyboard();
			ThicknessAnimation ta = new ThicknessAnimation();
			DoubleAnimation da = new DoubleAnimation();
			Storyboard.SetTargetProperty(ta, new PropertyPath("Margin"));
			Storyboard.SetTarget(ta, FileExecNotf);
			ta.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
			ta.DecelerationRatio = 1.0;
			Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
			Storyboard.SetTarget(da, FileExecNotf);
			da.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
			da.DecelerationRatio = 1.0;
			sb.Children.Add(ta);
			sb.Children.Add(da);
			isAnimatingNotf = true;
			ta.To = new Thickness(0.0, 0.0, 0.0, 16.0);
			da.To = 1.0;
			sb.Begin();
			await Task.Delay(1500);
			ta.To = new Thickness(0.0, 0.0, 0.0, 8.0);
			da.To = 0.0;
			sb.Begin();
			await Task.Delay(200);
			FileExecNotf.Visibility = Visibility.Hidden;
			isAnimatingNotf = false;
		}
	}

	private void FileContextMenu_LostFocus(object sender, RoutedEventArgs e)
	{
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void OpenHBOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (FileContextSelected is ExplorerEntry explorerEntry)
		{
			CreateTab(explorerEntry.EntryName.Content.ToString(), explorerEntry.Script);
		}
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void RenameHBOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void DeleteHBOpt_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
	{
		if (FileContextSelected is ExplorerEntry explorerEntry)
		{
			File.GetAttributes(explorerEntry.Path);
			if (explorerEntry.isFile)
			{
				if (File.Exists(explorerEntry.Path))
				{
					File.Delete(explorerEntry.Path);
				}
			}
			else if (Directory.Exists(explorerEntry.Path))
			{
				new DirectoryInfo(explorerEntry.Path).Delete(recursive: true);
			}
			CardHolder.Children.Remove(explorerEntry);
		}
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void ExplorerHBOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (FileContextSelected is ExplorerEntry explorerEntry)
		{
			Process.Start("explorer.exe", "/select, \"" + explorerEntry.Path + "\"");
		}
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void OpenHBOpt_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
	{
		if (FileContextSelected is ExplorerEntry { isFile: not false } explorerEntry && File.Exists(explorerEntry.Path))
		{
			Tab tab = CreateTab(explorerEntry.EntryName.Content.ToString(), explorerEntry.Script);
			tab.Path = explorerEntry.Path;
			tab.Select();
			BringUpMenu(isTabCaller: true);
		}
		if (FileContextSelected is CommunityEntry communityEntry)
		{
			CreateTab(communityEntry.EntryName.Content.ToString(), communityEntry.Script).Select();
			BringUpMenu(isTabCaller: true);
		}
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void ExecuteHBOpt_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
	{
		string script = "";
		if (FileContextSelected is ExplorerEntry explorerEntry)
		{
			script = explorerEntry.Script;
		}
		else if (FileContextSelected is CommunityEntry communityEntry)
		{
			script = communityEntry.Script;
		}
		if (Seliware.Execute(script))
		{
			DisplayNotification("File executed");
		}
		FileContextMenu.Visibility = Visibility.Hidden;
	}

	private void RenameHBOpt_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
	{
		if (FileContextSelected is ExplorerEntry explorerEntry)
		{
			explorerEntry.EntryName.Visibility = Visibility.Hidden;
			explorerEntry.EntryEdit.Text = explorerEntry.EntryName.Content.ToString();
			explorerEntry.EntryEdit.Visibility = Visibility.Visible;
			explorerEntry.Select();
			explorerEntry.Focus();
			explorerEntry.EntryEdit.Focus();
			Keyboard.Focus(explorerEntry.EntryEdit);
			explorerEntry.EntryEdit.SelectAll();
			FileContextMenu.Visibility = Visibility.Hidden;
		}
	}

	private void Entry_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Return)
		{
			return;
		}
		ExplorerEntry explorerEntry = (ExplorerEntry)((Grid)((Border)((Grid)((Grid)((TextBox)sender).Parent).Parent).Parent).Parent).Parent;
		if (explorerEntry.EntryEdit.Text == "")
		{
			explorerEntry.EntryName.Visibility = Visibility.Visible;
			explorerEntry.EntryEdit.Visibility = Visibility.Hidden;
			return;
		}
		explorerEntry.EntryName.Content = explorerEntry.EntryEdit.Text;
		explorerEntry.EntryName.Visibility = Visibility.Visible;
		explorerEntry.EntryEdit.Visibility = Visibility.Hidden;
		string text = Path.GetDirectoryName(explorerEntry.Path) + $"\\{explorerEntry.EntryName.Content}";
		if (explorerEntry.isFile)
		{
			if (!File.Exists(text))
			{
				File.Move(explorerEntry.Path, text);
			}
		}
		else if (!Directory.Exists(text))
		{
			Directory.Move(explorerEntry.Path, text);
		}
		explorerEntry.Path = text;
	}

	private void SaveOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		PromptSaveFile();
	}

	private void PromptSaveFile()
	{
		HBOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
		if (Common.SelectedTab == null)
		{
			return;
		}
		if (Common.SelectedTab.Path == "" || !File.Exists(Common.SelectedTab.Path))
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.DefaultExt = "lua";
			saveFileDialog.Filter = "Lua files (*.lua)|*.lua";
			if (saveFileDialog.ShowDialog() == true)
			{
				Common.SelectedTab.TabName = saveFileDialog.SafeFileName;
				Common.SelectedTab.Path = saveFileDialog.FileName;
				File.WriteAllText(saveFileDialog.FileName, ReadScript());
				Common.SelectedTab.isSaved = false;
				Common.SelectedTab.IsSaved = true;
			}
		}
		else
		{
			Common.SelectedTab.isSaved = false;
			Common.SelectedTab.IsSaved = true;
			Common.SelectedTab.TabName = Common.SelectedTab.TabName;
			Common.SelectedTab.watchSave = true;
			File.WriteAllText(Common.SelectedTab.Path, ReadScript());
		}
	}

	private void NewDraft_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (MainDirDisplay.EntryName.Content == "Scripts")
		{
			CreateDraft();
		}
	}

	private void CreateDraft()
	{
		ExplorerEntry explorerEntry = CreateExplorerCard("Untitled", "", new TimeSpan(0, 0, 0), CurrentDraftPath + "\\temp.bin");
		File.WriteAllText(CurrentDraftPath + "\\temp.bin", "");
		explorerEntry.EntryName.Visibility = Visibility.Hidden;
		explorerEntry.EntryEdit.Text = explorerEntry.EntryName.Content.ToString();
		explorerEntry.EntryEdit.Visibility = Visibility.Visible;
		explorerEntry.EntryEdit.PreviewKeyDown += Entry_PreviewKeyDown;
		Keyboard.ClearFocus();
		explorerEntry.EntryEdit.SelectAll();
		explorerEntry.EntryEdit.Focus();
		UIElement relativeTo = VisualTreeHelper.GetParent(explorerEntry) as UIElement;
		explorerEntry.TranslatePoint(new Point(0.0, 0.0), relativeTo);
		CardHolderScroller.ScrollToBottom();
		explorerEntry.Select();
	}

	private void UndoOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Undo();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
	}

	private void RedoOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Redo();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
	}

	private void MinimapOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		RegistryKey krnlSubkey = getKrnlSubkey();
		object value = krnlSubkey.GetValue("EditorMinimap", false);
		bool flag = value == null || !(value.ToString() == "true");
		if (sender != null)
		{
			krnlSubkey.SetValue("EditorMinimap", flag ? "true" : "false", RegistryValueKind.String);
		}
		else
		{
			flag = !flag;
		}
		svg2422.Visibility = ((!flag) ? Visibility.Hidden : Visibility.Visible);
		WebBrowserExtensions.ExecuteScriptAsyncWhenPageLoaded((IChromiumWebBrowserBase)(object)browser, "SwitchMinimap(" + flag.ToString().ToLower() + ");", true);
	}

	private void PrefHBOpt_MouseEnter(object sender, MouseEventArgs e)
	{
		PrefOpts.Visibility = Visibility.Visible;
		EditOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
		PrefHBOptOpen = 0;
	}

	private void PrefHBOpt_MouseLeave(object sender, MouseEventArgs e)
	{
		if (PrefHBOptOpen != 1)
		{
			PrefOpts.Visibility = Visibility.Hidden;
		}
	}

	private void PrefHBOptGate_MouseEnter(object sender, MouseEventArgs e)
	{
		if (PrefHBOptOpen == 0)
		{
			PrefHBOptOpen = 1;
		}
	}

	private void PrefOpts_MouseEnter(object sender, MouseEventArgs e)
	{
		PrefHBOptOpen = 2;
	}

	private void PrefOpts_MouseLeave(object sender, MouseEventArgs e)
	{
		PrefHBOptOpen = 0;
		PrefOpts.Visibility = Visibility.Hidden;
	}

	private void TopmostOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		base.Topmost = !base.Topmost;
		getKrnlSubkey().SetValue("Topmost", base.Topmost ? "true" : "false");
		svg242_Copy.Visibility = ((!base.Topmost) ? Visibility.Hidden : Visibility.Visible);
	}

	private void UnlockFPSOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		RegistryKey krnlSubkey = getKrnlSubkey();
		object value = krnlSubkey.GetValue("UnlockFPS");
		bool flag = value == null || !(value.ToString() == "true");
		if (sender != null)
		{
			krnlSubkey.SetValue("UnlockFPS", flag ? "true" : "false", RegistryValueKind.String);
		}
		else
		{
			flag = !flag;
		}
		Seliware.Execute("setfpscap(999)");
		svg242_Copy1.Visibility = ((!flag) ? Visibility.Hidden : Visibility.Visible);
	}

	private RegistryKey getKrnlSubkey()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE");
		RegistryKey registryKey2 = null;
		try
		{
			return registryKey.OpenSubKey("Krnl", writable: true);
		}
		catch
		{
			return registryKey.CreateSubKey("Krnl", writable: true);
		}
	}

	private void ViewHBOpt_MouseEnter(object sender, MouseEventArgs e)
	{
		ViewOpts.Visibility = Visibility.Visible;
		EditOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
		PrefOpts.Visibility = Visibility.Hidden;
		ViewHBOptOpen = 0;
	}

	private void ViewHBOpt_MouseLeave(object sender, MouseEventArgs e)
	{
		if (ViewHBOptOpen != 1)
		{
			ViewOpts.Visibility = Visibility.Hidden;
		}
	}

	private void ViewHBOptGate_MouseEnter(object sender, MouseEventArgs e)
	{
		if (ViewHBOptOpen == 0)
		{
			ViewHBOptOpen = 1;
		}
	}

	private void ViewOpts_MouseEnter(object sender, MouseEventArgs e)
	{
		ViewHBOptOpen = 2;
	}

	private void ViewOpts_MouseLeave(object sender, MouseEventArgs e)
	{
		ViewHBOptOpen = 0;
		ViewOpts.Visibility = Visibility.Hidden;
	}

	private void AutoAttachOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		AutoAttachEnabled = !AutoAttachEnabled;
		getKrnlSubkey().SetValue("AutoAttach", AutoAttachEnabled ? "true" : "false");
		svg242.Visibility = ((!AutoAttachEnabled) ? Visibility.Hidden : Visibility.Visible);
	}

	private void update_roblox_path()
	{
		try
		{
			string path = (((Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("ROBLOX Corporation") ?? throw new Exception()).OpenSubKey("Environments") ?? throw new Exception()).OpenSubKey("roblox-player") ?? throw new Exception()).GetValue("").ToString();
			roblox_path = Directory.GetParent(path).FullName;
		}
		catch (Exception)
		{
			roblox_path = "";
		}
	}

	private void AutoLaunchOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		update_roblox_path();
		if (roblox_path == "")
		{
			DisplayNotification("ROBLOX Player is not installed.");
			return;
		}
		AutoLaunchEnabled = !AutoLaunchEnabled;
		if (AutoLaunchEnabled)
		{
			enable_auto_launch();
		}
		else
		{
			disable_auto_launch();
		}
	}

	private void enable_auto_launch()
	{
		File.Exists(Path.Combine(roblox_path, "XInput1_4.dll"));
		auto_launch_mutex = new Mutex(initiallyOwned: false, "RJ_AL_MTX0001");
	}

	private void disable_auto_launch()
	{
		if (auto_launch_mutex != null)
		{
			auto_launch_mutex.Dispose();
		}
		bool createdNew;
		Mutex mutex = new Mutex(initiallyOwned: true, "RJ_AL_MTX0001", out createdNew);
		if (createdNew)
		{
			mutex.ReleaseMutex();
		}
		mutex.Dispose();
		string path = Path.Combine(roblox_path, "XInput1_4.dll");
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
				svg3.Visibility = Visibility.Hidden;
			}
			catch (Exception)
			{
				svg3.Visibility = Visibility.Visible;
				MessageBox.Show("Please close Roblox before trying to disable auto launch.", "Krnl", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}
	}

	private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
	{
		if (SearchInput.Text == "")
		{
			Searchable = false;
			SearchInput.Text = "Search";
			Searchable = true;
		}
	}

	private void CutOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Cut();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
		((UIElement)(object)browser).Focus();
	}

	private void CopyOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Copy();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
		((UIElement)(object)browser).Focus();
	}

	private void PasteOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Paste();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
		((UIElement)(object)browser).Focus();
	}

	private void DeleteOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "Delete();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
		((UIElement)(object)browser).Focus();
	}

	private void SelectAllOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		WebBrowserExtensions.EvaluateScriptAsync((IChromiumWebBrowserBase)(object)browser, "SelectAll();", (TimeSpan?)null, false).GetAwaiter().GetResult();
		HBOpts.Visibility = Visibility.Hidden;
		EditOpts.Visibility = Visibility.Hidden;
		((UIElement)(object)browser).Focus();
	}

	private void MainMenu_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void Workspace_MouseDown(object sender, MouseButtonEventArgs e)
	{
		Workspace.Focus();
	}

	private void HBOpts_LostFocus(object sender, RoutedEventArgs e)
	{
		HBOpts.Visibility = Visibility.Hidden;
	}

	private void OpenKrnlOpt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Process.Start(Directory.GetCurrentDirectory());
		HBOpts.Visibility = Visibility.Hidden;
		FileOpts.Visibility = Visibility.Hidden;
	}

	private void SearchBorder_MouseEnter(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.IBeam;
	}

	private void SearchBorder_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Arrow;
	}

	private void Editor_Drop(object sender, DragEventArgs e)
	{
		DataObject dataObject = (DataObject)e.Data;
		if (!dataObject.ContainsFileDropList())
		{
			return;
		}
		StringCollection fileDropList = dataObject.GetFileDropList();
		Tab tab = null;
		StringEnumerator enumerator = fileDropList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				try
				{
					tab = CreateTab(Path.GetFileName(current), File.ReadAllText(current));
				}
				catch (Exception ex)
				{
					tab = CreateTab("<ERROR> " + Path.GetFileName(current), "Couldn't access the file\nComputer produced error; " + ex.Message);
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		if (tab == null)
		{
			MessageBox.Show("FATAL CODE FAULT . Some code in the application produced unexpected output . 1941");
		}
		else
		{
			tab.Select();
		}
	}

	private void TabContainer_Drop(object sender, DragEventArgs e)
	{
		DataObject dataObject = (DataObject)e.Data;
		if (!dataObject.ContainsFileDropList())
		{
			return;
		}
		StringCollection fileDropList = dataObject.GetFileDropList();
		Tab tab = null;
		StringEnumerator enumerator = fileDropList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				tab = CreateTab(Path.GetFileName(current), File.ReadAllText(current), current);
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		tab?.Select();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		SaveTabs();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
