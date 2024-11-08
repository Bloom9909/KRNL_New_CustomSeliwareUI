using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KrnlUI;

public class ConsoleFramework
{
	public enum CharacterAttributes
	{
		FOREGROUND_BLUE = 1,
		FOREGROUND_GREEN = 2,
		FOREGROUND_RED = 4,
		FOREGROUND_INTENSITY = 8,
		BACKGROUND_BLUE = 0x10,
		BACKGROUND_GREEN = 0x20,
		BACKGROUND_RED = 0x40,
		BACKGROUND_INTENSITY = 0x80,
		COMMON_LVB_LEADING_BYTE = 0x100,
		COMMON_LVB_TRAILING_BYTE = 0x200,
		COMMON_LVB_GRID_HORIZONTAL = 0x400,
		COMMON_LVB_GRID_LVERTICAL = 0x800,
		COMMON_LVB_GRID_RVERTICAL = 0x1000,
		COMMON_LVB_REVERSE_VIDEO = 0x4000,
		COMMON_LVB_UNDERSCORE = 0x8000
	}

	public enum StdHandle
	{
		STD_INPUT_HANDLE = -10,
		STD_OUTPUT_HANDLE = -11,
		STD_ERROR_HANDLE = -12
	}

	public enum ConsoleMode : uint
	{
		ENABLE_ECHO_INPUT = 4u,
		ENABLE_EXTENDED_FLAGS = 128u,
		ENABLE_INSERT_MODE = 32u,
		ENABLE_LINE_INPUT = 2u,
		ENABLE_MOUSE_INPUT = 16u,
		ENABLE_PROCESSED_INPUT = 1u,
		ENABLE_QUICK_EDIT_MODE = 64u,
		ENABLE_WINDOW_INPUT = 8u,
		ENABLE_VIRTUAL_TERMINAL_INPUT = 512u,
		ENABLE_PROCESSED_OUTPUT = 1u,
		ENABLE_WRAP_AT_EOL_OUTPUT = 2u,
		ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4u,
		DISABLE_NEWLINE_AUTO_RETURN = 8u,
		ENABLE_LVB_GRID_WORLDWIDE = 16u
	}

	private const int HWND_TOPMOST = -1;

	private const int SWP_NOMOVE = 2;

	private const int SWP_NOSIZE = 1;

	private const int SW_HIDE = 0;

	private const int SW_SHOW = 5;

	private const int MF_BYCOMMAND = 0;

	private const int SC_CLOSE = 61536;

	private static IntPtr hConsole = GetStdHandle(-11);

	private static string _color = "@@LIGHT_GRAY@@";

	private static List<string> colors = new List<string>
	{
		"@@BLACK@@", "@@BLUE@@", "@@GREEN@@", "@@CYAN@@", "@@RED@@", "@@MAGENTA@@", "@@BROWN@@", "@@LIGHT_GRAY@@", "@@DARK_GRAY@@", "@@LIGHT_BLUE@@",
		"@@LIGHT_GREEN@@", "@@LIGHT_CYAN@@", "@@LIGHT_RED@@", "@@LIGHT_MAGENTA@@", "@@YELLOW@@", "@@WHITE@@"
	};

	private static int rlines = 0;

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int handle);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleMode(IntPtr phandle, int mode);

	[DllImport("kernel32.dll")]
	private static extern bool GetConsoleMode(IntPtr phandle, out int mode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, CharacterAttributes wAttributes);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

	public static void Init()
	{
		Console.CancelKeyPress += Console_CancelKeyPress;
		EnableVirtualTerminalProcessing();
		TopMost();
		DeleteMenu(GetSystemMenu(GetConsoleWindow(), bRevert: false), 61536, 0);
		Hide();
	}

	private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
	{
		e.Cancel = false;
	}

	private static void EnableVirtualTerminalProcessing()
	{
		GetConsoleMode(hConsole, out var mode);
		mode |= 4;
		SetConsoleMode(hConsole, mode);
		GetConsoleMode(GetStdHandle(-10), out mode);
		mode &= -65;
		mode |= 0x80;
		SetConsoleMode(GetStdHandle(-10), mode);
	}

	private static void Hide()
	{
		ShowWindow(GetConsoleWindow(), 0);
	}

	private static void Show()
	{
		ShowWindow(GetConsoleWindow(), 5);
	}

	private static void TopMost()
	{
		SetWindowPos(GetConsoleWindow(), new IntPtr(-1), 0, 0, 0, 0, 3);
	}

	private static void WriteLine(string text)
	{
		if (!setcolor(text))
		{
			Console.WriteLine(text);
		}
	}

	public static void Write(string text)
	{
		if (!setcolor(text))
		{
			Console.Write(text);
		}
	}

	private static string createcolor(int r = 0, int g = 0, int b = 0)
	{
		return $"\u001b[38;2;{r};{g};{b}m";
	}

	private static bool setcolor(string raw)
	{
		int num = colors.FindIndex(0, raw.Equals);
		if (num != -1)
		{
			SetConsoleTextAttribute(hConsole, (CharacterAttributes)num);
			_color = raw;
			return true;
		}
		return false;
	}

	private static bool strcmp(string s1, string s2)
	{
		return !(s1 == s2);
	}

	private static string eb64(string content)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
	}

	private static string db64(string content)
	{
		try
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(content));
		}
		catch
		{
			return content;
		}
	}

	private static void revertcolor(string color = "@@LIGHT_GRAY@@")
	{
		setcolor(color);
	}

	private static void interpret(string line)
	{
		List<string> list = line.Split(' ').ToList();
		string text = list[0].Replace(":", "");
		string value = db64(list[1]);
		string color = _color;
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 4:
			switch (text[3])
			{
			case 'T':
				if (text == "CCRT")
				{
					Show();
				}
				break;
			case 'R':
				if (!(text == "CCLR"))
				{
					if (text == "CERR")
					{
						Write("@@WHITE@@");
						Write("[");
						Write("@@LIGHT_RED@@");
						Write("ERROR");
						Write("@@WHITE@@");
						Write("] ");
						WriteLine(value);
						revertcolor(color);
					}
				}
				else
				{
					Console.Clear();
				}
				break;
			case 'P':
				if (!(text == "CINP"))
				{
					if (text == "CKYP")
					{
						Write("@@WHITE@@");
						Write("[");
						Write("@@LIGHT_GREEN@@");
						Write("KEY");
						Write("@@WHITE@@");
						Write("] ");
						WriteLine(value);
						revertcolor(color);
					}
				}
				else
				{
				}
				break;
			case 'N':
				if (text == "CWRN")
				{
					Write("@@WHITE@@");
					Write("[");
					Write("@@YELLOW@@");
					Write("WARNING");
					Write("@@WHITE@@");
					Write("] ");
					WriteLine(value);
					revertcolor(color);
				}
				break;
			case 'F':
				if (text == "CINF")
				{
					Write("@@WHITE@@");
					Write("[");
					Write("@@LIGHT_CYAN@@");
					Write("INFO");
					Write("@@WHITE@@");
					Write("] ");
					WriteLine(value);
					revertcolor(color);
				}
				break;
			case 'E':
				if (text == "CKYE")
				{
					Write("@@WHITE@@");
					Write("[");
					Write("@@LIGHT_RED@@");
					Write("KEY");
					Write("@@WHITE@@");
					Write("] ");
					WriteLine(value);
					if (value == "Incorrect key. Please get a key from https://cdn.krnl.place/getkey.php then restart the game and inject")
					{
						Process.Start("https://cdn.krnl.place/getkey.php");
					}
					revertcolor(color);
				}
				break;
			}
			break;
		case 5:
			switch (text[1])
			{
			case 'D':
				if (text == "CDTRY")
				{
					Console.Clear();
					setcolor("@@LIGHT_GRAY@@");
					Hide();
				}
				break;
			case 'N':
				if (text == "CNAME")
				{
					Console.Title = value;
					Show();
				}
				break;
			case 'P':
				if (text == "CPRNT")
				{
					Write(value);
				}
				break;
			case 'C':
				if (text == "SCLIP")
				{
					MainWindow.dispatcher.Invoke(delegate
					{
						Clipboard.SetText(value);
					});
				}
				break;
			}
			break;
		}
	}

	public static void TailFrom(string file)
	{
		Task.Run(async delegate
		{
			File.WriteAllText(file, "");
			while (true)
			{
				try
				{
					if (rlines == 0)
					{
						using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
						using StreamReader streamReader = new StreamReader(fileStream);
						string line;
						while ((line = streamReader.ReadLine()) != null)
						{
							rlines++;
							interpret(line);
						}
						streamReader.Dispose();
						fileStream.Dispose();
					}
					else if (rlines != 0)
					{
						List<string> list = File.ReadAllLines(file).ToList();
						list.RemoveRange(0, Math.Min(rlines, list.Count));
						File.WriteAllLines(file, list);
						rlines = 0;
					}
				}
				catch (Exception ex)
				{
					if (ex.StackTrace.IndexOf("RemoveRange") != -1)
					{
						rlines = 0;
					}
				}
				await Task.Delay(1);
			}
		});
	}
}
