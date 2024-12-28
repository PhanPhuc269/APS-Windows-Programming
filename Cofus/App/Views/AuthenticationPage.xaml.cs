using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using App.ViewModels;
using App.Views;
using System.Net.Mail;

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

    private async void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var emailDialog = new ContentDialog()
            {
                Title = "Quên mật khẩu",
                Content = new StackPanel
                {
                    Children =
                {
                    new TextBlock { Text = "Nhập tên đăng nhập và email của bạn để nhận mật khẩu mới.", Margin = new Thickness(0, 0, 0, 10) },
                    new TextBox { Name = "usernameTextBox", PlaceholderText = "Tên đăng nhập", Width = 300 },
                    new TextBox { Name = "emailTextBox", PlaceholderText = "Email", Width = 300 }
                }
                },
                PrimaryButtonText = "Gửi",
                CloseButtonText = "Hủy",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await emailDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var stackPanel = emailDialog.Content as StackPanel;
                var username = stackPanel?.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "usernameTextBox")?.Text;
                var email = stackPanel?.Children.OfType<TextBox>().FirstOrDefault(t => t.Name == "emailTextBox")?.Text;

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email))
                {
                    var user = _dao.GetUserByUsername(username);
                    if (user != null && user.Email == email)
                    {
                        var newPassword = GenerateTemporaryPassword();

                        // Cập nhật mật khẩu reset vào cơ sở dữ liệu
                        user.Password = newPassword;
                        var updated = _dao.UpdateUser(user);

                        if (updated)
                        {
                            // Gửi email chỉ khi cập nhật mật khẩu thành công
                            if (await SendResetEmailAsync(email, newPassword))
                            {
                                await new ContentDialog
                                {
                                    Title = "Thành công",
                                    Content = "Mật khẩu mới đã được gửi tới email của bạn.",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.Content.XamlRoot
                                }.ShowAsync();
                            }
                            else
                            {
                                await new ContentDialog
                                {
                                    Title = "Thất bại",
                                    Content = "Không thể gửi email. Vui lòng thử lại sau.",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.Content.XamlRoot
                                }.ShowAsync();
                            }
                        }
                        else
                        {
                            await new ContentDialog
                            {
                                Title = "Lỗi",
                                Content = "Không thể cập nhật mật khẩu. Vui lòng thử lại sau.",
                                CloseButtonText = "OK",
                                XamlRoot = this.Content.XamlRoot
                            }.ShowAsync();
                        }
                    }
                }
                else
                {
                    await new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng nhập đầy đủ tên đăng nhập và email.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    }.ShowAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
            await new ContentDialog
            {
                Title = "Error",
                Content = $"An error occurred: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            }.ShowAsync();
        }
    }

    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    private async Task<bool> SendResetEmailAsync(string email, string newPassword)
    {
        try
        {
            // Get the API key from the environment variable
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("SendGrid API key is missing.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("contact.quanminhle@gmail.com", "Cofus");
            var subject = "Reset Your Password";
            var to = new EmailAddress(email);

            // Plain text version for email clients that do not support HTML
            var plainTextContent = $"You requested to reset your password. Your new password is: {newPassword}.\n\nPlease log in and update your password as soon as possible.\n\nThank you,\nYour App Team";

            // HTML content
            var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; font-size: 16px; color: #333; text-align: center; padding: 20px;"">
                <h1 style=""color: #444;"">Hello,</h1>
                <p style=""font-size: 18px; color: #666;"">
                    You have requested to reset your password. Please find your new password below:
                </p>
                <p style=""font-size: 20px; font-weight: bold; color: #000; margin: 20px 0;"">
                    {newPassword}
                </p>

                <p style=""margin-top: 20px; font-size: 14px; color: #999;"">
                    If you did not request this reset, please contact our support team immediately.
                </p>
                <hr style=""margin: 30px 0; border: none; border-top: 1px solid #ddd;"" />
                <p style=""font-size: 14px; color: #999;"">
                    Thank you,<br />
    
                </p>
            </div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
            return false;
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
