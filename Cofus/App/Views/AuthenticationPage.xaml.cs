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
using App.Model;

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
    private string HashPassword(string password)
    {
        // Tạo salt ngẫu nhiên
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = password + salt;

        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var hash = Convert.ToBase64String(hashBytes);

            // Trả về hash + salt để lưu vào database
            return $"{hash}:{salt}";
        }
    }
    private bool VerifyPassword(string inputPassword, string storedHashedPassword)
    {
        // Tách hash và salt từ chuỗi lưu trong database
        var parts = storedHashedPassword.Split(':');
        if (parts.Length != 2) return false;

        var hash = parts[0];
        var salt = parts[1];

        // Hash lại mật khẩu người dùng nhập kèm với salt
        var saltedInputPassword = inputPassword + salt;
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedInputPassword));
            var inputHash = Convert.ToBase64String(hashBytes);

            // So sánh hash mới với hash lưu trong database
            return hash == inputHash;
        }
    }


    private bool CheckLogin(string user, string password)
    {
        var existingUser = _dao.GetUserByUsername(user);
        if (existingUser != null && VerifyPassword(password, existingUser.Password))
        {
            App.CurrentUser = existingUser; // Lưu thông tin người dùng vào biến toàn cục hoặc singleton
            return true;
        }
        return false;
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
            var dialog = new ContentDialog
            {
                XamlRoot = this.Content?.XamlRoot, // Kiểm tra XamlRoot
                Title = "Đăng nhập thành công",
                Content = $"Xin chào, {user}!",
                CloseButtonText = "OK"
            };

            if (dialog.XamlRoot != null)
            {
                await dialog.ShowAsync();
            }
            else
            {
                Console.WriteLine("XamlRoot is null. Cannot show ContentDialog.");
            }
        }
        else
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.Content?.XamlRoot, // Kiểm tra XamlRoot
                Content = "Tên người dùng hoặc mật khẩu không chính xác",
                CloseButtonText = "OK"
            };

            if (dialog.XamlRoot != null)
            {
                await dialog.ShowAsync();
            }
            else
            {
                Console.WriteLine("XamlRoot is null. Cannot show ContentDialog.");
            }
        }
    }



    private async void Signup_Click(object sender, RoutedEventArgs e)
    {
        var fullName = signupFullNameTextBox.Text;
        var username = signupUsernameTextBox.Text;
        var password = signupPasswordBox.Password;
        var confirmPassword = signupConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await ShowContentDialog("Thông báo", "Tên đăng nhập hoặc mật khẩu không thể trống.");
            return;
        }

        if (!IsPasswordValid(password))
        {
            await ShowContentDialog("Thông báo", "Mật khẩu phải chứa ít nhất 8 kí tự, gồm ít nhất 1 chữ hoa và ít nhất 1 kí tự đặc biệt.");
            return;
        }

        if (password != confirmPassword)
        {
            await ShowContentDialog("Thông báo", "Mật khẩu không trùng khớp.");
            return;
        }

        // Hash mật khẩu
        var hashedPassword = HashPassword(password);

        var user = new User
        {
            Name = fullName,
            Username = username,
            Password = hashedPassword // Lưu hash mật khẩu vào database
        };

        var result = _dao.AddUser(user);
        if (result)
        {
            await ShowContentDialog("Success", "Người dùng đăng kí thành công.");
            SwitchToSignIn(sender, e);
        }
        else
        {
            await ShowContentDialog("Error", $"Người dùng đăng kí thất bại. Tên đăng nhập '{username}' đã tồn tại.");
        }
    }




    private async Task ShowContentDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = this.Content.XamlRoot,
            Title = title,
            Content = content,
            CloseButtonText = "OK"
        };
        await dialog.ShowAsync();
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
    private bool IsPasswordValid(string password)
    {
        // Biểu thức chính quy kiểm tra mật khẩu:
        // - (?=.*[A-Z]): Ít nhất một chữ viết hoa.
        // - (?=.*[!@#$%^&*()_+]): Ít nhất một ký tự đặc biệt.
        // - .{8,}: Độ dài tối thiểu 8 ký tự.
        var passwordPattern = @"^(?=.*[A-Z])(?=.*[!@#$%^&*()_+]).{8,}$";
        bool isValid = System.Text.RegularExpressions.Regex.IsMatch(password, passwordPattern);
        Console.WriteLine($"Password: {password}, Valid: {isValid}");
        return isValid;
    }


}
