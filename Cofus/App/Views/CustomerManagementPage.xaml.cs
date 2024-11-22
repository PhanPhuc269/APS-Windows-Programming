using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using App.Model;
using System;
using System.Linq;
using ClosedXML.Excel; // Thêm thư viện này vào file
using System.IO;

namespace App.Views;

public sealed partial class CustomerManagementPage : Page
{
    public CustomerManagementViewModel ViewModel
    {
        get;
    }

    public CustomerManagementPage()
    {
        this.InitializeComponent();
        ViewModel = new CustomerManagementViewModel();
        InventoryListView.ItemsSource = ViewModel.FilteredCustomers;
    }
    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();
        var minPoints = string.IsNullOrEmpty(MinPointsTextBox.Text) ? (int?)null : int.Parse(MinPointsTextBox.Text);
        var maxPoints = string.IsNullOrEmpty(MaxPointsTextBox.Text) ? (int?)null : int.Parse(MaxPointsTextBox.Text);

        var selectedCriteria = SearchCriteriaComboBox.SelectedItem as ComboBoxItem;
        if (selectedCriteria != null)
        {
            switch (selectedCriteria.Content.ToString())
            {
                case "Name":
                    ViewModel.SearchCustomersByName(searchText, minPoints, maxPoints);
                    break;
                case "Phone Number":
                    ViewModel.SearchCustomersByPhoneNumber(searchText, minPoints, maxPoints);
                    break;
            }
        }
    }
    private void SearchByNameButton_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();
        int? minPoints = string.IsNullOrEmpty(MinPointsTextBox.Text) ? (int?)null : int.Parse(MinPointsTextBox.Text);
        int? maxPoints = string.IsNullOrEmpty(MaxPointsTextBox.Text) ? (int?)null : int.Parse(MaxPointsTextBox.Text);

        ViewModel.SearchCustomersByName(searchText, minPoints, maxPoints);
    }

    private void SearchByPhoneNumberButton_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();
        int? minPoints = string.IsNullOrEmpty(MinPointsTextBox.Text) ? (int?)null : int.Parse(MinPointsTextBox.Text);
        int? maxPoints = string.IsNullOrEmpty(MaxPointsTextBox.Text) ? (int?)null : int.Parse(MaxPointsTextBox.Text);

        ViewModel.SearchCustomersByPhoneNumber(searchText, minPoints, maxPoints);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        ClearInputFields();
        AddEditDialog.Title = "Thêm Nguyên Liệu";
        await AddEditDialog.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = (Customer)InventoryListView.SelectedItem;
        if (selectedCustomer != null)
        {
            CustomerIdTextBox.Text = selectedCustomer.CustomerId.ToString();
            CustomerNameTextBox.Text = selectedCustomer.CustomerName;
            PhoneNumberTextBox.Text = selectedCustomer.PhoneNumber;
            EmailTextBox.Text = selectedCustomer.Email;
            PointsTextBox.Text = selectedCustomer.Points.ToString();

            AddEditDialog.Title = "Sửa Nguyên Liệu";
            await AddEditDialog.ShowAsync();
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = (Customer)InventoryListView.SelectedItem;
        if (selectedCustomer != null)
        {
            ViewModel.Customers.Remove(selectedCustomer);
            ViewModel.UpdateCurrentPage();
        }
    }

    private async void DetailButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = (Customer)InventoryListView.SelectedItem;
        if (selectedCustomer != null)
        {
            var detailDialog = new ContentDialog
            {
                Title = "Chi tiết khách hàng",
                Content = $"Mã: {selectedCustomer.CustomerId}\n" +
                          $"Tên: {selectedCustomer.CustomerName}\n" +
                          $"Số điện thoại: {selectedCustomer.PhoneNumber}\n" +
                          $"Email: {selectedCustomer.Email}\n" +
                          $"Điểm: {selectedCustomer.Points}\n",
                CloseButtonText = "Đóng"
            };

            await detailDialog.ShowAsync();
        }
    }

    private void AddEditDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var newCustomer = new Customer
        {
            CustomerId = int.Parse(CustomerIdTextBox.Text),
            CustomerName = CustomerNameTextBox.Text,
            PhoneNumber = PhoneNumberTextBox.Text,
            Email = EmailTextBox.Text,
            Points = int.Parse(PointsTextBox.Text),
        };

        if ((string)AddEditDialog.Title == "Thêm Nguyên Liệu")
        {
            ViewModel.Customers.Add(newCustomer);
        }
        else if ((string)AddEditDialog.Title == "Sửa Nguyên Liệu")
        {
            var existingCustomer = ViewModel.Customers.FirstOrDefault(m => m.CustomerId == newCustomer.CustomerId);
            if (existingCustomer != null)
            {
                existingCustomer.CustomerName = newCustomer.CustomerName;
                existingCustomer.PhoneNumber = newCustomer.PhoneNumber;
                existingCustomer.Email = newCustomer.Email;
                existingCustomer.Points = newCustomer.Points;
            }
        }

        ViewModel.UpdateCurrentPage();
    }

    private void ClearInputFields()
    {
        CustomerIdTextBox.Text = string.Empty;
        CustomerNameTextBox.Text = string.Empty;
        PhoneNumberTextBox.Text = string.Empty;
        EmailTextBox.Text = string.Empty;
        PointsTextBox.Text = string.Empty;
    }

    private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPage > 0)
        {
            ViewModel.CurrentPage--;
            ViewModel.UpdateCurrentPage();
            PageInfoTextBlock.Text = $"Trang {ViewModel.CurrentPage + 1} ";
        }
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentPage < ViewModel.TotalPages() - 1)
        {
            ViewModel.CurrentPage++;
            ViewModel.UpdateCurrentPage();
            PageInfoTextBlock.Text = $"Trang {ViewModel.CurrentPage + 1} ";
        }
    }

    private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Bắt đầu xuất file Excel...");

            await Task.Run(() =>
            {
                try
                {
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "InventoryData.xlsx");

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Inventory");

                        // Tạo tiêu đề cột
                        worksheet.Cell(1, 1).Value = "Mã khách hàng";
                        worksheet.Cell(1, 2).Value = "Tên khách hàng";
                        worksheet.Cell(1, 3).Value = "Số điện thoại";
                        worksheet.Cell(1, 4).Value = "Email";
                        worksheet.Cell(1, 5).Value = "Điểm";
                        worksheet.Cell(1, 6).Value = "Phần thưởng";

                        var row = 2;
                        foreach (var customer in ViewModel.FilteredCustomers)
                        {
                            worksheet.Cell(row, 1).Value = customer.CustomerId;
                            worksheet.Cell(row, 2).Value = customer.CustomerName;
                            worksheet.Cell(row, 3).Value = customer.PhoneNumber;
                            worksheet.Cell(row, 4).Value = customer.Email;
                            worksheet.Cell(row, 5).Value = customer.Points;
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

            DispatcherQueue.TryEnqueue(async () =>
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
            DispatcherQueue.TryEnqueue(async () =>
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi xuất file Excel",
                    Content = $"Đã xảy ra lỗi khi xuất file Excel: {ex.Message}",
                    CloseButtonText = "Đóng"
                };
                await errorDialog.ShowAsync();
            });
        }
    }

    private void InventoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Logic to handle selection change
    }
}
