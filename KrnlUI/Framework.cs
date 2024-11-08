using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace KrnlUI;

internal class Framework
{
	public static class krnl_dll
	{
		public static bool developer_mode = true;

		public static int IsUpdated()
		{
			if (developer_mode)
			{
				return 0;
			}
			if (!File.Exists("krnl.dll"))
			{
				return -1;
			}
			string uWPHash = Roblox.GetUWPHash();
			if (uWPHash == null)
			{
				return -2;
			}
			string text = Request.DownloadString("https://k-storage.com/bootstrapper/uwp/" + uWPHash + "/krnl.checksum");
			if (text == null)
			{
				return -3;
			}
			if (!(text == GetHashSHA384("krnl.dll")))
			{
				return 1;
			}
			return 0;
		}

		public static bool Download()
		{
			if (developer_mode)
			{
				return true;
			}
			string uWPHash = Roblox.GetUWPHash();
			if (uWPHash == null)
			{
				return false;
			}
			return Request.DownloadFile("https://k-storage.com/bootstrapper/uwp/" + uWPHash + "/krnl.dll", "krnl.dll");
		}
	}

	public static class Roblox
	{
		public static string GetUWPPath()
		{
			string result = null;
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Classes").OpenSubKey("Local Settings")
					.OpenSubKey("Software")
					.OpenSubKey("Microsoft")
					.OpenSubKey("Windows")
					.OpenSubKey("CurrentVersion")
					.OpenSubKey("AppModel")
					.OpenSubKey("Repository")
					.OpenSubKey("Packages");
				string[] subKeyNames = registryKey.GetSubKeyNames();
				foreach (string text in subKeyNames)
				{
					if (text.Contains("ROBLOXCORPORATION.ROBLOX"))
					{
						result = registryKey.OpenSubKey(text).GetValue("PackageRootFolder").ToString();
					}
				}
			}
			catch (NullReferenceException)
			{
			}
			return result;
		}

		public static string GetUWPHash()
		{
			string uWPPath = GetUWPPath();
			if (uWPPath == null || !File.Exists(uWPPath + "\\Windows10Universal.exe"))
			{
				return null;
			}
			return GetHashSHA384(uWPPath + "\\Windows10Universal.exe");
		}
	}

	public static class Request
	{
		private static WebClient wc = new WebClient
		{
			Proxy = null
		};

		public static string DownloadString(string url)
		{
			try
			{
				return wc.DownloadString(url);
			}
			catch
			{
				return null;
			}
		}

		public static bool DownloadFile(string url, string path)
		{
			try
			{
				wc.DownloadFile(url, path);
				return true;
			}
			catch
			{
			}
			return false;
		}
	}

	public static string GetHashSHA384(string path)
	{
		using FileStream inputStream = File.OpenRead(path);
		return BitConverter.ToString(new SHA384Managed().ComputeHash(inputStream)).Replace("-", string.Empty).ToLower();
	}
}
