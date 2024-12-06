using App.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using PropertyChanged;
using App.ViewModels;
using DemoListBinding1610;
using System.ComponentModel;

namespace App.Views;

[AddINotifyPropertyChangedInterface]
public sealed partial class InvoiceControl : UserControl
{
    public InvoiceControlViewModel ViewModel { get; }

    public InvoiceControl()
    {
        this.InitializeComponent();
        ViewModel = new InvoiceControlViewModel();
    }

    private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Hiển thị hộp thoại lựa chọn phương thức thanh toán
        var paymentMethodDialog = new ContentDialog
        {
            Title = "Chọn phương thức thanh toán",
            Content = new ComboBox
            {
                ItemsSource = App.GetService<IDao>().GetAllPaymentMethod(),
                PlaceholderText = "Chọn phương thức...",
                HorizontalAlignment = HorizontalAlignment.Stretch
            },
            PrimaryButtonText = "Xác nhận",
            CloseButtonText = "Hủy"
        };
        paymentMethodDialog.XamlRoot = this.XamlRoot;
        var result = await paymentMethodDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var comboBox = (ComboBox)paymentMethodDialog.Content;
            if (comboBox.SelectedItem == null)
            {
                // Nếu chưa chọn phương thức nào, dừng lại
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn phương thức thanh toán.",
                    CloseButtonText = "OK"
                };
                errorDialog.XamlRoot = this.XamlRoot;
                await errorDialog.ShowAsync();
                return;
            }


            // Lấy phương thức thanh toán đã chọn
            var paymentMethod = comboBox.SelectedItem?.ToString();
            if (paymentMethod == null)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Phương thức thanh toán không hợp lệ.",
                    CloseButtonText = "OK"
                };
                
                errorDialog.XamlRoot = this.XamlRoot;
                await errorDialog.ShowAsync();
                return;
            }

            ViewModel.Invoice.PaymentMethod = paymentMethod;

            try
            {
                // Gọi phương thức Checkout trong ViewModel để xử lý thanh toán
                var isSuccess = await ViewModel.Checkout();

                if (isSuccess)
                {
                    // Thông báo khi thanh toán thành công
                    var dialog = new ContentDialog
                    {
                        Title = "Thanh toán thành công",
                        Content = $"Hóa đơn của bạn đã được lưu. Phương thức thanh toán: {paymentMethod}.",
                        CloseButtonText = "OK"
                    };
                    dialog.XamlRoot = this.XamlRoot;
                    await dialog.ShowAsync();
                    // Gửi yêu cầu đóng tab
                    
                }
                else
                {
                    // Thông báo lỗi nếu có lỗi xảy ra
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi thanh toán",
                        Content = "Đã có lỗi xảy ra trong quá trình thanh toán.",
                        CloseButtonText = "OK"
                    };

                    errorDialog.XamlRoot = this.XamlRoot;
                    await errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và thông báo lỗi
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi thanh toán",
                    Content = $"Đã có lỗi xảy ra: {ex.Message}",
                    CloseButtonText = "OK"
                };
                errorDialog.XamlRoot = this.XamlRoot;
                await errorDialog.ShowAsync();
            }
        }
        else
        {
            // Người dùng đã hủy bỏ, không thực hiện thanh toán
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if(ViewModel.IsPaid== true)
        {
            return;
        }
        // Kiểm tra xem nút đã được bấm có thuộc một sản phẩm nào
        if (sender is Button deleteButton && deleteButton.DataContext is InvoiceItem itemToDelete)
        {
            ViewModel.Invoice.RemoveItem(itemToDelete);
        }
    }
    private void QuantityTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        // Không cho phép thay đổi nếu IsPaid là true
        if (ViewModel.IsPaid)
        {
            args.Cancel = true;
            return;
        }
        // Chỉ cho phép ký tự số
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }

}

