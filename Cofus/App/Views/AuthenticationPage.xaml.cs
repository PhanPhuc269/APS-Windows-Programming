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
        var email = signupEmailTextBox.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
        {
            await ShowContentDialog("Thông báo", "Tên đăng nhập, Email hoặc mật khẩu không thể trống.");
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
            Password = hashedPassword, // Lưu hash mật khẩu vào database
            Email = email
        };

        var result = _dao.AddUser(user);
        if (result)
        {
            await ShowContentDialog("Success", "Người dùng đăng kí thành công.");
            SwitchToSignIn(sender, e);
        }
        else
        {
            await ShowContentDialog("Error", $"Người dùng đăng kí thất bại. Tên đăng nhập '{username}' hoặc Email '{email}' đã tồn tại.");
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
                        user.Password = HashPassword(newPassword) ;
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
        const string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string specialChars = "!@#$%^&*()_+";
        const string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string allChars = upperCaseChars + lowerCaseChars + digits + specialChars;
        var random = new Random();

        // Bảo đảm các điều kiện bắt buộc
        var password = new StringBuilder();
        password.Append(upperCaseChars[random.Next(upperCaseChars.Length)]); // Ít nhất 1 chữ viết hoa
        password.Append(specialChars[random.Next(specialChars.Length)]);    // Ít nhất 1 ký tự đặc biệt
        password.Append(lowerCaseChars[random.Next(lowerCaseChars.Length)]); // Ít nhất 1 chữ thường
        password.Append(digits[random.Next(digits.Length)]);                 // Ít nhất 1 chữ số

        // Điền thêm các ký tự ngẫu nhiên để đạt độ dài tối thiểu 8
        for (int i = password.Length; i < 8; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Xáo trộn các ký tự để đảm bảo tính ngẫu nhiên
        return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
    }

    private async Task<bool> SendResetEmailAsync(string email, string newPassword)
    {
        try
        {
            DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));
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
