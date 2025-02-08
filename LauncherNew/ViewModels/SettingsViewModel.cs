using System.Windows;

namespace LauncherNew.ViewModels;

public class SettingsViewModel
{
    public async Task HideLauncher()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        });
    }

    public async Task CloseLauncher()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.MainWindow.Close();
            
        });

    }
}