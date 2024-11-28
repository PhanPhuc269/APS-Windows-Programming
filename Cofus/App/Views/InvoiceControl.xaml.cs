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
        // Kiểm tra xem số điện thoại khách hàng có được nhập hay không
        if (!string.IsNullOrWhiteSpace(ViewModel.Invoice.CustomerPhoneNumber))
        {
            // Kiểm tra xem số điện thoại khách hàng có tồn tại trong cơ sở dữ liệu hay không
            var customerPhoneNumbers = await App.GetService<IDao>().SuggestCustomerPhoneNumbers(ViewModel.Invoice.CustomerPhoneNumber);
            if (!customerPhoneNumbers.Contains(ViewModel.Invoice.CustomerPhoneNumber))
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Số điện thoại khách hàng không tồn tại.",
                    CloseButtonText = "OK"
                };
                errorDialog.XamlRoot = this.XamlRoot;
                await errorDialog.ShowAsync();
                return;
            }
        }
        if (ViewModel.Invoice.InvoiceItems == null || !ViewModel.Invoice.InvoiceItems.Any())
        {
            var errorDialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = "Vui lòng chọn món.",
                CloseButtonText = "OK"
            };
            errorDialog.XamlRoot = this.XamlRoot;
            await errorDialog.ShowAsync();
            return;
        }
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
    private async void CustomerPhoneNumber_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (ViewModel.IsPaid)
        {
            // Khôi phục giá trị ban đầu nếu hóa đơn đã thanh toán
            sender.Text = ViewModel.Invoice.CustomerPhoneNumber;
            return;
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            // Lọc chỉ cho phép nhập số
            string input = sender.Text;
            string filteredInput = new string(input.Where(char.IsDigit).ToArray());

            if (input != filteredInput)
            {
                // Cập nhật lại văn bản chỉ chứa số
                sender.Text = filteredInput;
            }
            else
            {
                // Fetch suggestions từ cơ sở dữ liệu
                var suggestions = await FetchCustomerPhoneNumberSuggestionsAsync(filteredInput);
                sender.ItemsSource = suggestions;
                
            }
        }
        if(ViewModel.Invoice.CustomerPhoneNumber != null && ViewModel.Invoice.CustomerPhoneNumber != "")
        {
            // Cập nhật điểm tiêu dùng
            int CurenPoints = App.GetService<IDao>().GetComsumedPoints(sender.Text);
            ViewModel.ConsumedPoints = (CurenPoints > ViewModel.Invoice.TotalPrice && ViewModel.Invoice.TotalPrice!=0) ? ViewModel.Invoice.TotalPrice : CurenPoints;
        }
    }


    private void CustomerPhoneNumber_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            // Use the chosen suggestion
            sender.Text = args.ChosenSuggestion.ToString();
            int CurenPoints = App.GetService<IDao>().GetComsumedPoints(sender.Text);
            ViewModel.ConsumedPoints= CurenPoints>ViewModel.Invoice.TotalPrice ? ViewModel.Invoice.TotalPrice : CurenPoints ;
        }
    }

    private Task<List<string>> FetchCustomerPhoneNumberSuggestionsAsync(string query)
    {
        return App.GetService<IDao>().SuggestCustomerPhoneNumbers(query);
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            if ( ViewModel.ConsumedPoints <= 0 || (ViewModel.Invoice.InvoiceItems == null || !ViewModel.Invoice.InvoiceItems.Any())) toggleSwitch.IsOn = false;
            else if (toggleSwitch.IsOn)
            {
                int CurenPoints = App.GetService<IDao>().GetComsumedPoints(ViewModel.Invoice.CustomerPhoneNumber);
                ViewModel.Invoice.ConsumedPoints = CurenPoints > ViewModel.Invoice.TotalPrice ? ViewModel.Invoice.TotalPrice : CurenPoints;
            }
            else
            {
                ViewModel.Invoice.ConsumedPoints = 0;
            }
        }
    }
}
