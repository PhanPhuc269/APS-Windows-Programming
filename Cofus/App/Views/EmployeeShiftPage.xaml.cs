using App.Model;
using System.Collections.ObjectModel;
using App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace App.Views;

public sealed partial class EmployeeShiftPage : Page
{
    public EmployeeShiftViewModel ViewModel
    {
        get;
    }

    // Biến thành viên để lưu trữ mã xác thực
    private string _authCode;

    public EmployeeShiftPage()
    {
        this.InitializeComponent();
        ViewModel = new EmployeeShiftViewModel();
    }

    public async void OnShiftClick(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var now = DateTime.Now;

        // Hiển thị thông tin ca chiều
        ShiftInfoTextBlock.Text = $"Ngày: {now.Date:dd/MM/yyyy}, Ca: Chiều, Giờ: 13:00 - 17:55";

        // Hiển thị dialog
        var result = await CheckInDialog.ShowAsync();
        CheckInDialog.Closing += Dialog_Closing;

        if (result == ContentDialogResult.Primary)
        {
            // Lấy dữ liệu mã nhân viên và mã xác thực
            var usernameEmployee = EmployeeCodeTextBox.Text;
            var authCode = AuthCodePasswordBox.Password;

            // Gửi thông tin chấm công
            ProcessCheckIn(now, usernameEmployee, authCode, _authCode);
        }
    }

    private async void OnSendAuthCodeClick(object sender, RoutedEventArgs e)
    {
        var usernameEmployee = EmployeeCodeTextBox.Text;
        if (string.IsNullOrEmpty(usernameEmployee))
        {
            ShiftInfoError.Text = "Vui lòng nhập tên đăng nhập của nhân viên.";
            return;
        }
        var email = App.GetService<IDao>().GetUserByUsername(usernameEmployee)?.Email;
        if (email == null)
        {
            ShiftInfoError.Text = "Không tìm thấy tài khoản hoặc email không hợp lệ";
            return;
        }    

        // Tạo mã xác thực
        _authCode = GenerateAuthCode();

        // Gửi email mã xác thực
        var success = await ViewModel.SendAuthCodeAsync(email, _authCode);
        if (success)
        {
            ShiftInfoError.Text = "Mã xác thực đã được gửi.";
        }
        else
        {
            ShiftInfoError.Text = "Gửi mã xác thực thất bại.";
        }
    }

    private async void ProcessCheckIn(DateTime shiftDate, string usernameEmployee, string authCode, string Code)
    {
        // Kiểm tra mã nhân viên và mã xác thực
        if (string.IsNullOrEmpty(usernameEmployee) || string.IsNullOrEmpty(authCode))
        {
            // Hiển thị lỗi nếu thông tin không đầy đủ
            ShowError("Vui lòng nhập đầy đủ mã nhân viên và mã xác thực.");
            return;
        }
        var User = App.GetService<IDao>().GetUserByUsername(usernameEmployee);
        if (User == null)
        {
            // Hiển thị lỗi nếu mã nhân viên không đúng
            ShowError("Username nhân viên không đúng.");
            return;
        }
        if (Code == null)
        {
            // Hiển thị lỗi nếu mã xác thực không tồn tại
            ShowError("Mã xác thực chưa được gửi.");
            return;
        }
        if (authCode != Code)
        {
            // Hiển thị lỗi nếu mã xác thực không đúng
            ShowError("Mã xác thực không đúng.");
            return;
        }
        //Kiểm tra nhân viên này đã chấm công chưa
        

        var now = DateTime.Now;

        // Kiểm tra mã nhân viên và mã xác thực
        if (string.IsNullOrEmpty(usernameEmployee) || string.IsNullOrEmpty(AuthCodePasswordBox.Password))
        {
            ShowError("Vui lòng nhập đầy đủ mã nhân viên và mã xác thực.");
            return;
        }

        // Determine if the current time is morning or afternoon
        bool isAfternoon = now.TimeOfDay >= new TimeSpan(12, 0, 0);

        var shift = new Shift
        {
            ShiftDate = now,
            MorningShift = !isAfternoon,
            AfternoonShift = isAfternoon,
        };
        if (await App.GetService<IDao>().CheckShiftAttendance(int.Parse(User.Id), shift))
        {
            ShowError("Bạn đã chấm công cho ca này. Không thể chấm công lại");
            return;
        }

        bool success = await ViewModel.CheckInShift(int.Parse(User.Id), shift);

        if (success)
        {
            ShowError("");
            CheckInDialog.Hide();
            ViewModel.LoadShiftAttendance();

            // Hiển thị thông báo thành công
            var dialog = new ContentDialog
            {
                Title = "Thành công",
                Content = "Chấm công thành công.",
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        else
        {
            // Hiển thị thông báo lỗi
            ShowError("Có lỗi xảy ra khi chấm công.");
        }
    }

    private async void ShowError(string message)
    {
        //ShiftInfoError.Text = message;
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "Close",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private string GenerateAuthCode()
    {
        var random = new Random();
        int authCode = random.Next(100000, 999999); // Tạo số ngẫu nhiên từ 100000 đến 999999
        return authCode.ToString();
    }

    private async void OnCheckInDialogConfirm(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        //var now = DateTime.Now;
        //var usernameEmployee = EmployeeCodeTextBox.Text;

        //// Kiểm tra mã nhân viên và mã xác thực
        //if (string.IsNullOrEmpty(usernameEmployee) || string.IsNullOrEmpty(AuthCodePasswordBox.Password))
        //{
        //    ShowError("Vui lòng nhập đầy đủ mã nhân viên và mã xác thực.");
        //    return;
        //}

        //// Determine if the current time is morning or afternoon
        //bool isAfternoon = now.TimeOfDay >= new TimeSpan(12, 0, 0);

        //var shift = new Shift
        //{
        //    ShiftDate = now,
        //    MorningShift = !isAfternoon,
        //    AfternoonShift = isAfternoon,
        //};

        //bool success = await ViewModel.CheckInShift(int.Parse(usernameEmployee), shift);

        //if (success)
        //{
        //    ShowError("");
        //    CheckInDialog.Hide();
        //    ViewModel.LoadShiftAttendance();

        //    // Hiển thị thông báo thành công
        //    var dialog = new ContentDialog
        //    {
        //        Title = "Thành công",
        //        Content = "Chấm công thành công.",
        //        CloseButtonText = "Đóng",
        //        XamlRoot = this.XamlRoot
        //    };
        //    await dialog.ShowAsync();
        //}
        //else
        //{
        //    args.Cancel = true;
        //    // Hiển thị thông báo lỗi
        //    ShowError("Có lỗi xảy ra khi chấm công.");
        //}
    }

    private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        // Ngăn không cho dialog đóng nếu chưa thỏa mãn điều kiện
        if (args.Result == ContentDialogResult.Primary && (string.IsNullOrEmpty(EmployeeCodeTextBox.Text) || string.IsNullOrEmpty(AuthCodePasswordBox.Password)))
        {
            args.Cancel = true; // Ngăn đóng dialog
        }
    }

    private void OnCheckInDialogCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Handle the cancel button click event
    }
}
