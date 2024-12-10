using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using App.Model;
using System;
using System.Linq;
using ClosedXML.Excel; // Thêm thư viện này vào file
using System.IO;
using Microsoft.UI.Xaml.Media;

namespace App.Views;

public sealed partial class InventoryManagementPage : Page
{
    public InventoryManagementViewModel ViewModel
    {
        get;
    }

    public InventoryManagementPage()
    {
        this.InitializeComponent();
        ViewModel = new InventoryManagementViewModel();
        InventoryListView.ItemsSource = ViewModel.FilteredMaterials;
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string searchText = SearchBox.Text.ToLower();
        DateTime? startExpirationDate = StartExpirationDatePicker.SelectedDate?.Date;
        DateTime? endExpirationDate = EndExpirationDatePicker.SelectedDate?.Date;

        ViewModel.SearchMaterials(searchText, startExpirationDate, endExpirationDate);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        ClearInputFields();
        AddEditDialog.Title = "Thêm Nguyên Liệu";
        ImportDatePicker.Visibility = Visibility.Visible;
        await AddEditDialog.ShowAsync();
    }
    private bool _isDialogOpen = false;

    private async void ShowNotification(string message)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot // Thay thế this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị dialog: {ex.Message}");
        }
    }



    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedMaterial = (Material)InventoryListView.SelectedItem;
        if (selectedMaterial != null)
        {
            MaterialCodeTextBox.Text = selectedMaterial.MaterialCode;
            MaterialNameTextBox.Text = selectedMaterial.MaterialName;
            QuantityTextBox.Text = selectedMaterial.Quantity.ToString();
            CategoryTextBox.Text = selectedMaterial.Category;
            UnitTextBox.Text = selectedMaterial.Unit;
            UnitPriceTextBox.Text = selectedMaterial.UnitPrice.ToString();
            ImportDatePicker.Visibility = Visibility.Collapsed;
            ExpirationDatePicker.Date = new DateTimeOffset(selectedMaterial.ExpirationDate);
                        
            AddEditDialog.Title = "Sửa Nguyên Liệu";
            
            await AddEditDialog.ShowAsync();
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedMaterial = (Material)InventoryListView.SelectedItem;
        if (selectedMaterial != null)
        {
            App.GetService<IDao>().DeleteMaterial(selectedMaterial.MaterialCode);
            ViewModel.AllMaterials.Remove(selectedMaterial);
            ViewModel.UpdateCurrentPage();
        }
    }

    private void DetailButton_Click(object sender, RoutedEventArgs e)
    {
        // Logic for handling detail view
        var selectedMaterial = (Material)InventoryListView.SelectedItem;
        if (selectedMaterial != null)
        {
            // Implement logic to display detailed information about the selected material
            ContentDialog detailDialog = new ContentDialog
            {
                Title = "Chi tiết Nguyên Liệu",
                Content = $"Mã: {selectedMaterial.MaterialCode}\n" +
                          $"Tên: {selectedMaterial.MaterialName}\n" +
                          $"Số lượng: {selectedMaterial.Quantity}\n" +
                          $"Phân loại: {selectedMaterial.Category}\n" +
                          $"Đơn vị: {selectedMaterial.Unit}\n" +
                          $"Đơn giá: {selectedMaterial.UnitPrice}\n" +
                          $"Ngày nhập: {selectedMaterial.FormattedImportDate}\n" +
                          $"Hạn sử dụng: {selectedMaterial.FormattedExpirationDate}",
                CloseButtonText = "Đóng"
            };

            detailDialog.ShowAsync();
        }
    }
    private async void AddEditDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var newMaterial = new Material
        {
            MaterialCode = MaterialCodeTextBox.Text,
            MaterialName = MaterialNameTextBox.Text,
            Quantity = int.Parse(QuantityTextBox.Text),
            Category = CategoryTextBox.Text,
            Unit = UnitTextBox.Text,
            UnitPrice = int.Parse(UnitPriceTextBox.Text),
            ExpirationDate = ExpirationDatePicker.Date.DateTime
        };

        if (AddEditDialog.Title == "Thêm Nguyên Liệu")
        {
            newMaterial.ImportDate = DateTime.Now;
            App.GetService<IDao>().AddMaterial(newMaterial);
            ViewModel.AllMaterials.Add(newMaterial);
        }
        else if (AddEditDialog.Title == "Sửa Nguyên Liệu")
        {
            var existingMaterial = ViewModel.AllMaterials.FirstOrDefault(m => m.MaterialCode == newMaterial.MaterialCode);
            if (existingMaterial != null)
            {
                existingMaterial.MaterialName = newMaterial.MaterialName;
                existingMaterial.Quantity = newMaterial.Quantity;
                existingMaterial.Category = newMaterial.Category;
                existingMaterial.Unit = newMaterial.Unit;
                existingMaterial.UnitPrice = newMaterial.UnitPrice;
                existingMaterial.ExpirationDate = newMaterial.ExpirationDate;
                App.GetService<IDao>().UpdateMaterial(newMaterial);
            }
            if (existingMaterial.Threshold > newMaterial.Quantity)
            {
                AddEditDialog.Hide(); // Ensure the AddEditDialog is closed
                ShowNotification($"Số lượng nguyên liệu {newMaterial.MaterialName} hiện tại dưới ngưỡng cảnh báo");
            }
        }

        ViewModel.UpdateCurrentPage();
    }


    private void ClearInputFields()
    {
        MaterialCodeTextBox.Text = string.Empty;
        MaterialNameTextBox.Text = string.Empty;
        QuantityTextBox.Text = string.Empty;
        CategoryTextBox.Text = string.Empty;
        UnitTextBox.Text = string.Empty;
        UnitPriceTextBox.Text = string.Empty;
        ImportDatePicker.Date = DateTimeOffset.Now;
        ExpirationDatePicker.Date = DateTimeOffset.Now.AddMonths(1);
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
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "InventoryData.xlsx");

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Inventory");

                        // Tạo tiêu đề cột
                        worksheet.Cell(1, 1).Value = "Mã nguyên liệu";
                        worksheet.Cell(1, 2).Value = "Tên nguyên liệu";
                        worksheet.Cell(1, 3).Value = "Số lượng";
                        worksheet.Cell(1, 4).Value = "Phân loại";
                        worksheet.Cell(1, 5).Value = "Đơn vị tính";
                        worksheet.Cell(1, 6).Value = "Đơn giá";
                        worksheet.Cell(1, 7).Value = "Ngày nhập";
                        worksheet.Cell(1, 8).Value = "Hạn sử dụng";

                        int row = 2;
                        foreach (var material in ViewModel.FilteredMaterials)
                        {
                            worksheet.Cell(row, 1).Value = material.MaterialCode;
                            worksheet.Cell(row, 2).Value = material.MaterialName;
                            worksheet.Cell(row, 3).Value = material.Quantity;
                            worksheet.Cell(row, 4).Value = material.Category;
                            worksheet.Cell(row, 5).Value = material.Unit;
                            worksheet.Cell(row, 6).Value = material.UnitPrice;
                            worksheet.Cell(row, 7).Value = material.ImportDate.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 8).Value = material.ExpirationDate.ToString("dd/MM/yyyy");
                            row++;
                        }

                        var range = worksheet.RangeUsed();
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        range.Style.Font.Bold = false;

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

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
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
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
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
    private async void Set_Notification_Threshold_Click(object sender, RoutedEventArgs e)
    {
        MaterialsListView.Visibility = Visibility.Visible;
        await EditMaterialsDialog.ShowAsync();
        var materialsBelowThreshold = ViewModel.AllMaterials
            .Where(material => material.Quantity < material.Threshold)
            .ToList();

        EditMaterialsDialog.Hide();

        if (materialsBelowThreshold.Any())
        {
            string notificationMessage = "Các nguyên liệu dưới ngưỡng cảnh báo:\n";
            foreach (var material in materialsBelowThreshold)
            {
                notificationMessage += $"- {material.MaterialName} (Số lượng: {material.Quantity}, Ngưỡng: {material.Threshold})\n";
            }

            ShowNotification(notificationMessage);
        }
    }
    private async void UpdateThresholdButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        foreach (var material in ViewModel.AllMaterials)
        {
            var thresholdTextBox = FindChild<TextBox>(MaterialsListView, "ThresholdTextBox", material.MaterialCode);
            if (thresholdTextBox == null)
            {
                System.Diagnostics.Debug.WriteLine($"ThresholdTextBox for material {material.MaterialCode} not found.");
                continue;
            }

            if (!int.TryParse(thresholdTextBox.Text, out int newThreshold))
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse threshold for material {material.MaterialCode}.");
                continue;
            }

            if (material.Threshold != newThreshold)
            {
                material.Threshold = newThreshold;
                var success = App.GetService<IDao>().UpdateMaterialThreshold(material.MaterialCode, newThreshold);
                if (!success)
                {
                    ShowNotification($"Failed to update threshold for {material.MaterialName}");
                }
            }
        }

        ShowNotification("Thresholds updated successfully.");
    }

    private T FindChild<T>(DependencyObject parent, string childName = null, string materialCode = null)
    where T : DependencyObject
    {
        if (parent == null) return null;

        // If parent is already of the target type and meets the conditions, return it
        if (parent is T targetType)
        {
            if (!string.IsNullOrEmpty(childName) &&
                parent is FrameworkElement fe &&
                fe.Name != childName)
                return null;

            if (!string.IsNullOrEmpty(materialCode) &&
                parent is TextBox textBox &&
                textBox.Text != materialCode)
                return null;

            return targetType;
        }

        // Recursively search through the children
        T foundChild = null;
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            foundChild = FindChildRecursive<T>(child, childName, materialCode, 5);
            if (foundChild != null)
                break;
        }

        return foundChild;
    }

    private T FindChildRecursive<T>(DependencyObject parent, string childName, string materialCode, int maxDepth)
        where T : DependencyObject
    {
        if (parent == null || maxDepth <= 0) return null;

        if (parent is T targetType)
        {
            if (!string.IsNullOrEmpty(childName) &&
                parent is FrameworkElement fe &&
                fe.Name != childName)
                return null;

            if (!string.IsNullOrEmpty(materialCode) &&
                parent is TextBox textBox &&
                textBox.Text != materialCode)
                return null;

            return targetType;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindChildRecursive<T>(child, childName, materialCode, maxDepth - 1);
            if (result != null)
                return result;
        }

        return null;
    }




    private int GetUpdatedThresholdForMaterial(string materialCode)
    {
        // Assuming you have a TextBox named ThresholdTextBox in the EditMaterialsDialog
        // and a way to map the materialCode to the corresponding TextBox
        var thresholdTextBox = EditMaterialsDialog.FindName($"ThresholdTextBox_{materialCode}") as TextBox;

        if (thresholdTextBox != null && int.TryParse(thresholdTextBox.Text, out int updatedThreshold))
        {
            return updatedThreshold;
        }

        // Return a default value or handle the case where the TextBox is not found or the value is invalid
        return 0; // Replace with actual default value or error handling
    }

    private void InventoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Logic to handle selection change
    }
   

}