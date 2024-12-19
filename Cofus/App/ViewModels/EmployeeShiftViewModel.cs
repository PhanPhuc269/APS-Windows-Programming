﻿using CommunityToolkit.Mvvm.ComponentModel;
using App.Model;
using System.Collections.ObjectModel;

namespace App.ViewModels;

public partial class EmployeeShiftViewModel : ObservableRecipient
{
    // Danh sách chấm công
    public ObservableCollection<ShiftAttendance> ShiftAttendances { get; private set; }

    // Ngày bắt đầu tuần hiện tại
    public DateTime CurrentWeekStartDate { get; private set; }

    // Ngày kết thúc tuần hiện tại
    public DateTime CurrentWeekEndDate { get; private set; }

    // Giờ bắt đầu ca sáng (VD: 8:00 sáng)
    private readonly TimeSpan MorningShiftStartTime = new TimeSpan(8, 0, 0);

    // Giờ bắt đầu ca chiều (VD: 13:00 chiều)
    private readonly TimeSpan AfternoonShiftStartTime = new TimeSpan(13, 0, 0);

    public EmployeeShiftViewModel()
    {
        ShiftAttendances = new ObservableCollection<ShiftAttendance>();

        // Thiết lập tuần hiện tại
        SetWeek(DateTime.Now);

        // Tải dữ liệu chấm công
        LoadShiftAttendance();
    }

    // Thiết lập tuần dựa trên ngày tham chiếu
    public void SetWeek(DateTime referenceDate)
    {
        int delta = DayOfWeek.Monday - referenceDate.DayOfWeek;
        CurrentWeekStartDate = referenceDate.AddDays(delta);
        CurrentWeekEndDate = CurrentWeekStartDate.AddDays(6);
    }

    // Tải dữ liệu chấm công
    private void LoadShiftAttendance()
    {
        var shiftAttendances = App.GetService<IDao>().GetShiftAttendances(CurrentWeekStartDate, CurrentWeekEndDate);
        ShiftAttendances.Clear();
        foreach (var attendance in shiftAttendances)
        {
            ShiftAttendances.Add(attendance);
        }
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
}