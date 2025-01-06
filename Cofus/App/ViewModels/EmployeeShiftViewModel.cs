using CommunityToolkit.Mvvm.ComponentModel;
using App.Model;
using System.Collections.ObjectModel;
using PropertyChanged;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace App.ViewModels;
[AddINotifyPropertyChangedInterface]
public partial class EmployeeShiftViewModel : ObservableRecipient
{
    // Danh sách chấm công
    public ObservableCollection<ShiftAttendance> ShiftAttendances { get; private set; }

    // Ngày bắt đầu tuần hiện tại
    public DateTime CurrentWeekStartDate { get; private set; }

    // Ngày kết thúc tuần hiện tại
    public DateTime CurrentWeekEndDate { get; private set; }

    private DateTimeOffset? selectedDate;
    public DateTimeOffset? SelectedDate
    {
        get => selectedDate;
        set
        {
            selectedDate = value;
            OnSelectedDateChanged();
        }
    }


    // Giờ bắt đầu ca sáng (VD: 8:00 sáng)
    private readonly TimeSpan MorningShiftStartTime = new TimeSpan(8, 0, 0);

    // Giờ bắt đầu ca chiều (VD: 13:00 chiều)
    private readonly TimeSpan AfternoonShiftStartTime = new TimeSpan(13, 0, 0);

    // Các thuộc tính ngày trong tuần
    public string Day1 { get; private set; }
    public string Day2 { get; private set; }
    public string Day3 { get; private set; }
    public string Day4 { get; private set; }
    public string Day5 { get; private set; }
    public string Day6 { get; private set; }
    public string Day7 { get; private set; }

    public EmployeeShiftViewModel()
    {
        ShiftAttendances = new ObservableCollection<ShiftAttendance>();

        // Thiết lập tuần hiện tại
        if (SelectedDate.HasValue)
        {
            SetWeek(SelectedDate.Value.DateTime);
        }
        else
        {
            SetWeek(DateTime.Now);
        }

        // Tải dữ liệu chấm công
        LoadShiftAttendance();
    }

    // Thiết lập tuần dựa trên ngày tham chiếu
    public void SetWeek(DateTime referenceDate)
    {
        // Nếu ngày tham chiếu là Chủ nhật, lùi lại 6 ngày để lấy thứ Hai của tuần hiện tại
        if (referenceDate.DayOfWeek == DayOfWeek.Sunday)
        {
            referenceDate = referenceDate.AddDays(-6);
        }
        else
        {
            int delta = DayOfWeek.Monday - referenceDate.DayOfWeek;
            referenceDate = referenceDate.AddDays(delta);
        }

        CurrentWeekStartDate = referenceDate.Date;
        CurrentWeekEndDate = CurrentWeekStartDate.AddDays(6);
        UpdateDays();
    }

    // Cập nhật các thuộc tính ngày trong tuần
    private void UpdateDays()
    {
        Day1 = CurrentWeekStartDate.ToString("ddd dd/MM");
        Day2 = CurrentWeekStartDate.AddDays(1).ToString("ddd dd/MM");
        Day3 = CurrentWeekStartDate.AddDays(2).ToString("ddd dd/MM");
        Day4 = CurrentWeekStartDate.AddDays(3).ToString("ddd dd/MM");
        Day5 = CurrentWeekStartDate.AddDays(4).ToString("ddd dd/MM");
        Day6 = CurrentWeekStartDate.AddDays(5).ToString("ddd dd/MM");
        Day7 = CurrentWeekStartDate.AddDays(6).ToString("ddd dd/MM");
        OnPropertyChanged(nameof(Day1));
        OnPropertyChanged(nameof(Day2));
        OnPropertyChanged(nameof(Day3));
        OnPropertyChanged(nameof(Day4));
        OnPropertyChanged(nameof(Day5));
        OnPropertyChanged(nameof(Day6));
        OnPropertyChanged(nameof(Day7));
    }

    // Tải dữ liệu chấm công
    public void LoadShiftAttendance()
    {
        var shAttend = App.GetService<IDao>().GetShiftAttendances(CurrentWeekStartDate, CurrentWeekEndDate);
        ShiftAttendances.Clear();
        foreach (var attendance in shAttend)
        {
            foreach (var shift in attendance.Shifts)
            {
                // Tính toán cột hiển thị dựa trên ngày trong tuần
                shift.ColumnIndex = (shift.ShiftDate.Date - CurrentWeekStartDate.Date).Days;

                shift.IsMorningEnabled = GetShiftEnableState(shift.ShiftDate, MorningShiftStartTime);
                shift.IsAfternoonEnabled = GetShiftEnableState(shift.ShiftDate, AfternoonShiftStartTime);

            }
            ShiftAttendances.Add(attendance);
        }
    }

    private bool GetShiftEnableState(DateTime shiftDate, TimeSpan shiftStartTime)
    {
        var now = DateTime.Now;

        if (shiftDate.Date < now.Date) return false; // Ngày đã qua
        if (shiftDate.Date > now.Date) return false; // Ngày tương lai

        // Cho phép trước và sau 15 phút của giờ ca làm việc
        var shiftStart = shiftDate.Date + shiftStartTime;
        return now >= shiftStart.AddMinutes(-15) && now <= shiftStart.AddMinutes(15);
    }

    // Cập nhật trạng thái chấm công
    public async Task<bool> CheckInShift(int employeeId, Shift shift)
    {
        if (shift.MorningShift)
        {
            // Kiểm tra trễ ca sáng
            if (shift.ShiftDate.TimeOfDay > MorningShiftStartTime)
            {
                shift.Note = "Chấm công trễ ca sáng";
            }
        }
        else
        {
            // Kiểm tra trễ ca chiều
            if (shift.ShiftDate.TimeOfDay > AfternoonShiftStartTime)
            {
                shift.Note = "Chấm công trễ ca chiều";
            }
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        return await SaveAttendanceToDatabase( employeeId,  shift);
    }

    // Lưu chấm công vào cơ sở dữ liệu
    public async Task<bool> SaveAttendanceToDatabase(int employeeId, Shift shift)
    {
        return await App.GetService<IDao>().AddShiftAttendance(shift, employeeId);
    }
    // Xử lý khi SelectedDate thay đổi
    private void OnSelectedDateChanged()
    {
        if (SelectedDate.HasValue)
        {
            SetWeek(SelectedDate.Value.DateTime);
            LoadShiftAttendance();
        }
    }

    public async Task<bool> SendAuthCodeAsync(string email, string authCode)
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
            var subject = "Mã xác thực chấm công";
            var to = new EmailAddress(email);

            // Plain text version for email clients that do not support HTML
            var plainTextContent = $"Bạn đã yêu cầu mã xác thực để chấm công. Mã xác thực của bạn là: {authCode}.\n\nVui lòng sử dụng mã này để hoàn tất quá trình chấm công.\n\nCảm ơn,\nĐội ngũ Cofus";

            // HTML content
            var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; font-size: 16px; color: #333; text-align: center; padding: 20px;"">
                <h1 style=""color: #444;"">Xin chào,</h1>
                <p style=""font-size: 18px; color: #666;"">
                    Bạn đã yêu cầu mã xác thực để chấm công. Vui lòng tìm mã xác thực của bạn bên dưới:
                </p>
                <p style=""font-size: 20px; font-weight: bold; color: #000; margin: 20px 0;"">
                    {authCode}
                </p>

                <p style=""margin-top: 20px; font-size: 14px; color: #999;"">
                    Nếu bạn không yêu cầu mã xác thực này, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi ngay lập tức.
                </p>
                <hr style=""margin: 30px 0; border: none; border-top: 1px solid #ddd;"" />
                <p style=""font-size: 14px; color: #999;"">
                    Cảm ơn,<br />
                    Đội ngũ Cofus
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
}
