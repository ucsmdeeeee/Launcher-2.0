using System.Windows.Controls;
using LauncherNew.Views.Pages;
using MvvmCross.ViewModels;

namespace LauncherNew.ViewModels;

public class MainViewModel:MvxViewModel
{
    private Page _dashboardPage = new DashboardPage();
    private Page _shopPage = new ShopPage();
    private Page _settingsPage = new SettingsPage();
    private Page _profilePage = new ProfilePage();


    private Page _currentPage;

    public Page CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public MainViewModel()
    {
        CurrentPage = new Views.Pages.AuthorizationPage();
    }

    public void ShowLauncher()
    {
        CurrentPage = _dashboardPage;
    }


   
    public void ShowShop()
    {
        CurrentPage = _shopPage;
    }

    public void ShowSettings()
    {
        CurrentPage = _settingsPage;
    }
    
    public void ShowProfile()
    {
        CurrentPage = _profilePage;
    }
    
}