using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using App.Model;
using System;
using System.Linq;
using ClosedXML.Excel; // Thêm thư viện này vào file
using System.IO;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI;

namespace App.Views;

public sealed partial class EmployeeManagementPage : Page
{
    public EmployeeManagementViewModel ViewModel
    {
        get;
    }

    public EmployeeManagementPage()
    {
        ViewModel = App.GetService<EmployeeManagementViewModel>();
        InitializeComponent();
    }

    private void Search_User_Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SearchEmployees(SearchBox.Text);
    }

    private void Delete_User_Button_Click(object sender, RoutedEventArgs e)
    {
        var selectedUser = (User)UserListView.SelectedItem;
        if (selectedUser != null)
        {
            App.GetService<IDao>().DeleteEmployee(selectedUser.Id);
            ViewModel.Employee.Remove(selectedUser);
            ViewModel.UpdateCurrentPage();
        }
        
    }
    
    private void AddEditDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var user = new User
        {
            Id = EmployeeCodeTextBox.Text,
            Name = EmployeeNameTextBox.Text,
            Role = FunctionTextBox.Text,
            AccessLevel = int.Parse(AccessLevelTextBox.Text),
            Username = ViewModel.SelectedEmployee.Username,
            Password = ViewModel.SelectedEmployee.Password
        };
        ViewModel.UpdateEmployee(user);
    }

    private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UserListView.SelectedItem is User selectedUser)
        {
            ViewModel.SelectedEmployee = selectedUser;
            EmployeeCodeTextBox.Text = selectedUser.Id;
            EmployeeNameTextBox.Text = selectedUser.Name;
            FunctionTextBox.Text = selectedUser.Role;
            AccessLevelTextBox.Text = selectedUser.AccessLevel.ToString();
        }
    }
    private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPage > 1)
        {
            ViewModel.CurrentPage--;
            ViewModel.LoadEmployees();
        }
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPage < ViewModel.TotalPages)
        {
            ViewModel.CurrentPage++;
            ViewModel.LoadEmployees();
        }
    }

    private async void ExportExcel_User_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Bắt đầu xuất file Excel...");

            await Task.Run(() =>
            {
                try
                {
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "UserData.xlsx");

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Users");

                        // Tạo tiêu đề cột
                        worksheet.Cell(1, 1).Value = "Mã nhân viên";
                        worksheet.Cell(1, 2).Value = "Tên nhân viên";
                        worksheet.Cell(1, 3).Value = "Vai trò";
                        worksheet.Cell(1, 4).Value = "Cấp độ truy cập";

                        var row = 2;
                        foreach (var user in ViewModel.FilteredUser)
                        {
                            worksheet.Cell(row, 1).Value = user.Id;
                            worksheet.Cell(row, 2).Value = user.Name;
                            worksheet.Cell(row, 3).Value = user.Role;
                            worksheet.Cell(row, 4).Value = user.AccessLevel;
                            row++;
                        }

                        var range = worksheet.RangeUsed();
                        if (range != null)
                        {
                            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            range.Style.Font.Bold = false;
                        }

                        workbook.SaveAs(filePath);
                    }

                    System.Diagnostics.Debug.WriteLine("Xuất file Excel thành công.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi khi ghi file: {ex.Message}");
                    throw; // Ném lại ngoại lệ để xử lý bên ngoài
                }
            });

            await DispatcherQueue.EnqueueAsync(async () =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Xuất Excel thành công",
                    Content = $"File Excel đã được lưu tại {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}",
                    CloseButtonText = "Đóng"
                };
                await dialog.ShowAsync();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi không mong muốn: {ex.Message}");
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi xuất file Excel",
                    Content = $"Đã xảy ra lỗi khi xuất file Excel: {ex.Message}",
                    CloseButtonText = "Đóng"
                };
            });
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedEmployee != null)
        {
            EmployeeCodeTextBox.Text = ViewModel.SelectedEmployee.Id;
            EmployeeNameTextBox.Text = ViewModel.SelectedEmployee.Name;
            FunctionTextBox.Text = ViewModel.SelectedEmployee.Role;
            AccessLevelTextBox.Text = ViewModel.SelectedEmployee.AccessLevel.ToString();
            AddEditDialog.Title = "Sửa Thông Tin";
            AddEditDialog.ShowAsync();
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        EmployeeCodeTextBox.Text = string.Empty;
        EmployeeNameTextBox.Text = string.Empty;
        FunctionTextBox.Text = string.Empty;
        AccessLevelTextBox.Text = string.Empty;
        AddEditDialog.Title = "Thêm Nhân Viên";
        AddEditDialog.ShowAsync();
    }


}