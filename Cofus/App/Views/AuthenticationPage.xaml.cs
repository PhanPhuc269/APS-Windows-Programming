using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;
using App.ViewModels;
using App.Views;

namespace App.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AuthenticationPage : Page
{

    private IDao _dao;
    public AuthenticationPage()
    {
        this.InitializeComponent();
        RequestedTheme = ElementTheme.Light;
        _dao = App.GetService<IDao>();
        System.Diagnostics.Debug.WriteLine("AuthenticationPage loaded successfully.");

    }

    private void SwitchToSignUp(object sender, RoutedEventArgs e)
    {
        SignInPanel.Visibility = Visibility.Collapsed;
        SignUpPanel.Visibility = Visibility.Visible;
    }

    private void SwitchToSignIn(object sender, RoutedEventArgs e)
    {
        SignInPanel.Visibility = Visibility.Visible;
        SignUpPanel.Visibility = Visibility.Collapsed;
    }

    private bool CheckLogin(string user, string password)
    {

        var existingUser = _dao.GetUserByUsername(user);
        return existingUser != null && existingUser.Password == password;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("user"))
            {
                usernameTextBox.Text = localSettings.Values["user"].ToString();
                var encryptedPasswordInBase64 = localSettings.Values["password"]?.ToString();

                if (!string.IsNullOrEmpty(encryptedPasswordInBase64))
                {
                    var password = await DecryptPasswordAsync(encryptedPasswordInBase64);
                    passwordBox.Password = password;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var user = usernameTextBox.Text;
        var password = passwordBox.Password;

        if (CheckLogin(user, password))
        {
            if (rememberCheckBox.IsChecked == true)
            {
                var encryptedPassword = await EncryptPasswordAsync(password);

                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["user"] = user;
                localSettings.Values["password"] = encryptedPassword;
            }

            // Chuyển đến trang chính của ứng dụng
            UIElement? shell = App.GetService<ShellPage>();
            App.MainWindow.Content = shell ?? new Frame();

            // Hiển thị cửa sổ thông báo với username
            await new ContentDialog()
            {
                XamlRoot = this.Content.XamlRoot,
                Title = "Đăng nhập thành công",
                Content = $"Xin chào, {user}!",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
        else
        {
            await new ContentDialog()
            {
                XamlRoot = this.Content.XamlRoot,
                Content = "Tên người dùng hoặc mật khẩu không chính xác",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
    }


    private async void Signup_Click(object sender, RoutedEventArgs e)
    {
        var user = usernameTextBox.Text;
        var password = passwordBox.Password;

        if (CheckLogin(user, password))
        {
            if (rememberCheckBox.IsChecked == true)
            {
                var encryptedPassword = await EncryptPasswordAsync(password);

                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["user"] = user;
                localSettings.Values["password"] = encryptedPassword;
            }

            UIElement? shell = App.GetService<ShellPage>();
            App.MainWindow.Content = shell ?? new Frame();
        }
        else
        {
            await new ContentDialog()
            {
                XamlRoot = this.Content.XamlRoot,
                Content = "Incorrect username or password entered\n",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
    }
    
    private async Task<string> EncryptPasswordAsync(string password)
    {
        var provider = new DataProtectionProvider("LOCAL=user");
        IBuffer plainBuffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
        IBuffer encryptedBuffer = await provider.ProtectAsync(plainBuffer);
        return CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
    }

    private async Task<string> DecryptPasswordAsync(string encryptedPassword)
    {
        var provider = new DataProtectionProvider();
        IBuffer encryptedBuffer = CryptographicBuffer.DecodeFromBase64String(encryptedPassword);
        IBuffer plainBuffer = await provider.UnprotectAsync(encryptedBuffer);
        return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, plainBuffer);
    }
}
