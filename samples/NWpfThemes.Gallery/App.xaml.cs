using System;
using System.IO;
using System.Windows;

namespace NWpfThemes.Gallery;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        int index = Array.IndexOf(e.Args, "--screenshots");
        if (index < 0)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
            return;
        }

        string folder = index + 1 < e.Args.Length ? e.Args[index + 1] : "screenshots";
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            Screenshots.CaptureAll(folder);
            Shutdown(0);
        }
        catch (Exception ex)
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "error.txt"), ex.ToString());
            Shutdown(1);
        }
    }
}
