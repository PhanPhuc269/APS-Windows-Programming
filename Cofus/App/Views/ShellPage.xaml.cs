using App.Contracts.Services;
using App.Helpers;
using App.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.System;

namespace App.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }
    public string Greeting => $"Hi, {App.CurrentUser.Username}";
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

        Loaded += ShellPage_Loaded;
    }
    private void ConfigureNavigationMenu(string role)
    {
        // Định nghĩa quyền truy cập của từng vai trò
        var adminVisibleTags = new HashSet<string>
    {
        "CustomerManagementPage",
        "InventoryManagementPage",
        "EmployeeManagementPage",
        "RevenuePage",
        "EmployeeShiftPage",
        "ProductManagement"
    };

        var staffVisibleTags = new HashSet<string>
    {
        "SalePage",
        "CustomerManagementPage",
        "InventoryManagementPage",
        "ProductManagement",
        "EmployeeShiftPage"
    };

        // Duyệt qua tất cả các mục trong NavigationViewControl
        foreach (var item in NavigationViewControl.MenuItems)
        {
            if (item is NavigationViewItem navigationViewItem)
            {
                string tag = navigationViewItem.Tag?.ToString();

                // Quyết định hiển thị dựa trên vai trò
                if (role == "Admin")
                {
                    navigationViewItem.Visibility = adminVisibleTags.Contains(tag) ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (role == "Staff")
                {
                    navigationViewItem.Visibility = staffVisibleTags.Contains(tag) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // Ẩn tất cả các mục nếu vai trò không hợp lệ
                    navigationViewItem.Visibility = Visibility.Collapsed;
                }
            }
        }
    }


    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        //foreach (var item in NavigationViewControl.MenuItems)
        //{
        //    if (item is NavigationViewItem navigationViewItem && navigationViewItem.Tag.ToString() == "SalePage")
        //    {
        //        NavigationViewControl.SelectedItem = navigationViewItem;
        //        ViewModel.NavigationService.NavigateTo("App.ViewModels.SalePageViewModel");
        //        break;
        //    }
        //}
        ConfigureNavigationMenu(App.CurrentUser?.Role);
        string role = App.CurrentUser?.Role;

        foreach (var item in NavigationViewControl.MenuItems)
        {
            
            if (role =="Admin" && item is NavigationViewItem navigationViewItem && navigationViewItem.Tag.ToString() == "CustomerManagementPage")
            {
                NavigationViewControl.SelectedItem = navigationViewItem;
                ViewModel.NavigationService.NavigateTo("App.ViewModels.CustomerManagementViewModel");
                break;
            }
            if (role == "Staff" && item is NavigationViewItem navigationViewItem2 && navigationViewItem2.Tag.ToString() == "SalePage")
            {
                NavigationViewControl.SelectedItem = navigationViewItem2;
                ViewModel.NavigationService.NavigateTo("App.ViewModels.SalePageViewModel");
                break;
            }
        }

    }
    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Đăng xuất",
            Content = "Bạn có muốn đăng xuất?",
            PrimaryButtonText = "OK",
            CloseButtonText = "Hủy",
            XamlRoot = this.Content.XamlRoot // Set the XamlRoot property
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Clear user data
            App.CurrentUser = null;


            UIElement? shell = App.GetService<AuthenticationPage>();
            App.MainWindow.Content = shell ?? new Frame();
        }
    }


}
