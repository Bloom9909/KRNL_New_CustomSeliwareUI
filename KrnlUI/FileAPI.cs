using System;
using System.Runtime.InteropServices;

namespace KrnlUI;

internal class FileAPI
{
	[Flags]
	public enum FileOperationFlags : ushort
	{
		FOF_SILENT = 4,
		FOF_NOCONFIRMATION = 0x10,
		FOF_ALLOWUNDO = 0x40,
		FOF_SIMPLEPROGRESS = 0x100,
		FOF_NOERRORUI = 0x400,
		FOF_WANTNUKEWARNING = 0x4000
	}

	public enum FileOperationType : uint
	{
		FO_MOVE = 1u,
		FO_COPY,
		FO_DELETE,
		FO_RENAME
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct SHFILEOPSTRUCT
	{
		public IntPtr hwnd;

		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;

		public string pFrom;

		public string pTo;

		public FileOperationFlags fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public IntPtr hNameMappings;

		public string lpszProgressTitle;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	public static bool MoveToRecycleBin(string path)
	{
		try
		{
			SHFILEOPSTRUCT sHFILEOPSTRUCT = default(SHFILEOPSTRUCT);
			sHFILEOPSTRUCT.wFunc = FileOperationType.FO_DELETE;
			sHFILEOPSTRUCT.pFrom = path + "\0\0";
			sHFILEOPSTRUCT.fFlags = FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOERRORUI;
			SHFILEOPSTRUCT FileOp = sHFILEOPSTRUCT;
			SHFileOperation(ref FileOp);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
