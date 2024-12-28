﻿using App.Model;
using System.Collections.ObjectModel;
using App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace App.Views;

public sealed partial class EmployeeShiftPage : Page
{
    public EmployeeShiftViewModel ViewModel
    {
        get;
    }
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
            var employeeCode = EmployeeCodeTextBox.Text;
            var authCode = AuthCodePasswordBox.Password;

            // Gửi thông tin chấm công
            ProcessCheckIn(now, false, employeeCode, authCode);
        }
    }

    private void ProcessCheckIn(DateTime shiftDate, bool isMorningShift, string employeeCode, string authCode)
    {
        // Kiểm tra mã nhân viên và mã xác thực
        if (string.IsNullOrEmpty(employeeCode) || string.IsNullOrEmpty(authCode))
        {
            // Hiển thị lỗi nếu thông tin không đầy đủ
            ShowError("Vui lòng nhập đầy đủ mã nhân viên và mã xác thực.");
            return;
        }

        // Gửi dữ liệu chấm công (có thể gọi API hoặc cập nhật cơ sở dữ liệu tại đây)
        Console.WriteLine($"Chấm công: Ngày {shiftDate:dd/MM/yyyy}, Ca: {(isMorningShift ? "Sáng" : "Chiều")}, Mã nhân viên: {employeeCode}");
    }

    private void ShowError(string message)
    {
        ShiftInfoError.Text = message;
    }
    private async void OnCheckInDialogConfirm(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var now = DateTime.Now;
        var employeeCode = EmployeeCodeTextBox.Text;
        var authCode = AuthCodePasswordBox.Password;

        // Kiểm tra mã nhân viên và mã xác thực
        if (string.IsNullOrEmpty(employeeCode) || string.IsNullOrEmpty(authCode))
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

        bool success = await ViewModel.CheckInShift(int.Parse(employeeCode), shift);

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
            args.Cancel = true;
            // Hiển thị thông báo lỗi
            ShowError("Có lỗi xảy ra khi chấm công.");
        }
    }
    private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        // Ngăn không cho dialog đóng nếu chưa thỏa mãn điều kiện
        if (args.Result == ContentDialogResult.Primary &&( string.IsNullOrEmpty(EmployeeCodeTextBox.Text) || string.IsNullOrEmpty(AuthCodePasswordBox.Password)))
        {
            args.Cancel = true; // Ngăn đóng dialog
        }
    }
    private void OnCheckInDialogCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Handle the cancel button click event
    }
}