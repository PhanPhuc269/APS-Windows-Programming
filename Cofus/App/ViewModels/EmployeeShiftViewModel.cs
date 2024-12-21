using CommunityToolkit.Mvvm.ComponentModel;
using App.Model;
using System.Collections.ObjectModel;
using PropertyChanged;

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

        CurrentWeekStartDate = referenceDate;
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
    public void UpdateShiftAttendance(int employeeId, DateTime shiftDate, bool isMorningShift, bool newStatus, DateTime checkInTime)
    {
        var attendance = ShiftAttendances.FirstOrDefault(sa =>
            sa.EmployeeId == employeeId && sa.Shifts.Any(s => s.ShiftDate.Date == shiftDate.Date));

        if (attendance != null)
        {
            var shift = attendance.Shifts.FirstOrDefault(s => s.ShiftDate.Date == shiftDate.Date);
            if (shift != null)
            {
                if (isMorningShift)
                {
                    shift.MorningShift = newStatus;

                    // Kiểm tra trễ ca sáng
                    if (newStatus && checkInTime.TimeOfDay > MorningShiftStartTime)
                    {
                        shift.Note = "Chấm công trễ ca sáng";
                    }
                }
                else
                {
                    shift.AfternoonShift = newStatus;

                    // Kiểm tra trễ ca chiều
                    if (newStatus && checkInTime.TimeOfDay > AfternoonShiftStartTime)
                    {
                        shift.Note = "Chấm công trễ ca chiều";
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                SaveAttendanceToDatabase(attendance);
            }
        }
    }

    // Lưu chấm công vào cơ sở dữ liệu
    private void SaveAttendanceToDatabase(ShiftAttendance attendance)
    {
        // Implement the logic to save the attendance to the database
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
}
