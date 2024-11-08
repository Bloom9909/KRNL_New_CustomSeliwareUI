using SeliwareAPI;
using System;
using System.Windows;

namespace KrnlUI;

public partial class App : Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        Seliware.Initialize();
        app.Run(new MainWindow());
    }
}