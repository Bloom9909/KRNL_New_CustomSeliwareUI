// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// KrnlUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// KrnlUI.Pipes
using System;
using System.IO;
using System.Runtime.InteropServices;
using KrnlUI;

internal class Pipes
{
	private static string PipeName = "krnlpipe";

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool WaitNamedPipe(string name, int timeout);

	public static bool PipeActive()
	{
		try
		{
			if (!WaitNamedPipe(Path.GetFullPath("\\\\.\\pipe\\" + PipeName), 0))
			{
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
