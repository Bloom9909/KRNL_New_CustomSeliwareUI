using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using SeliwareAPI;
using CefSharp.Wpf;

namespace KrnlUI;

public class Injector
{
    public static MainWindow main;
    public static bool inject_status()
    {
        return Seliware.IsInjected();
    }

    public static void run_script()
    {
        Seliware.Execute(main.ReadScript());
    }
    public static void injection()
    {
        Seliware.Inject();
    }
}