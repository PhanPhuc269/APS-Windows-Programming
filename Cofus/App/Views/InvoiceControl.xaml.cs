using App.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using PropertyChanged;
using App.ViewModels;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Security.Cryptography;
using QRCoder;
using System.Diagnostics;
using Windows.Storage;
using System.Net;
using System.Threading;
using Windows.ApplicationModel.Payments;
using Newtonsoft.Json;
using Windows.Web.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Drawing;
using Windows.UI.WebUI;
using System.Web;
using MySqlX.XDevAPI;
using App.Services;
using App.Helpers;
using App.Services.Payment;


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
        // Kiểm tra xem hóa đơn có món nào không
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
                bool isPaid=false;
                IPaymentMethod paymentMethodInstance = PaymentFactory.CreatePaymentMethod(paymentMethod);
                isPaid = await paymentMethodInstance.ProcessPayment(ViewModel.Invoice);
                //ContentDialog checkoutDia;
                //if (paymentMethod== "MoMo")
                //{
                //    checkoutDia = await MoMoPayment.ShowMoMoQRCode(ViewModel.Invoice);
                //    checkoutDia.XamlRoot = this.XamlRoot;
                //    var dialogResult = ContentDialogResult.None;
                //    dialogResult = await checkoutDia.ShowAsync();
                //    if (dialogResult == ContentDialogResult.Primary)
                //    {
                //        isPaid = true;
                //    }
                //    else
                //    {
                //        isPaid = false;
                //    }
                //}
                //if (paymentMethod == "VNPay")
                //{
                //    checkoutDia = await VNPayPayment.ShowVNPayQRCode(ViewModel.Invoice);
                //    checkoutDia.XamlRoot=this.XamlRoot;
                //    var dialogResult = ContentDialogResult.None;
                //    dialogResult = await checkoutDia.ShowAsync();
                //    if (dialogResult == ContentDialogResult.Primary)
                //    {
                //        isPaid = true;
                //    }
                //    else
                //    {
                //        isPaid = false;
                //    }
                //}
                //if (paymentMethod == "VietQR")
                //{
                //    VietQRPayment vietQRPayment = new VietQRPayment();
                //    var token = GenerateUniqueOrderId(); // Tạo mã đơn hàng duy nhất
                //    var qrUrl = vietQRPayment.GenerateVietQR(ViewModel.Invoice, token); // Tạo URL mã QR

                //    // Hiển thị ContentDialog chứa mã QR
                //    checkoutDia = await OpenPaymentWebViewDialog(qrUrl, true);

                //    // Mở dialog (đối với WebView hoặc bất kỳ cách nào hiển thị mã QR)
                //    var dialogTask = checkoutDia.ShowAsync(); // Chờ dialog hiển thị

                //    // Kiểm tra trạng thái thanh toán bất đồng bộ
                //    isPaid = await vietQRPayment.WaitForPaymentAsync(ViewModel.Invoice, token);

                //    // Kiểm tra nếu thanh toán đã thành công, và nếu thành công, đóng dialog
                //    if (isPaid)
                //    {
                //        checkoutDia.Hide(); // Đóng dialog khi thanh toán thành công
                //    }
                //    await dialogTask;

                //}
                //if (paymentMethod == "Cash")
                //{
                //    // Create a dialog to input the received amount
                //    // Tạo một tham chiếu đến TextBox
                //    TextBox receivedAmountTextBox = new TextBox { PlaceholderText = "Amount" };

                //    ContentDialog cashDialog = new ContentDialog
                //    {
                //        Title = "Cash Payment",
                //        Content = new StackPanel
                //        {
                //            Children =
                //            {
                //                new TextBlock { Text = "Enter the amount received:" },
                //                receivedAmountTextBox // Thêm TextBox trực tiếp vào đây
                //            }
                //        },
                //        PrimaryButtonText = "Confirm",
                //        CloseButtonText = "Cancel",
                //        XamlRoot = this.XamlRoot
                //    };

                //    var dialogResult = await cashDialog.ShowAsync();

                //    if (dialogResult == ContentDialogResult.Primary)
                //    {
                //        // Truy cập giá trị trực tiếp từ TextBox
                //        if (decimal.TryParse(receivedAmountTextBox.Text, out decimal receivedAmount))
                //        {
                //            decimal totalAmount = ViewModel.Invoice.TotalPrice;
                //            decimal change = receivedAmount - totalAmount;

                //            if (change >= 0)
                //            {
                //                // Đánh dấu hóa đơn là đã thanh toán
                //                ViewModel.Invoice.MarkAsPaid();

                //                // Hiển thị thông báo thành công
                //                ContentDialog successDialog = new ContentDialog
                //                {
                //                    Title = "Payment Successful",
                //                    Content = $"Payment received. Change: {change:C}",
                //                    CloseButtonText = "OK",
                //                    XamlRoot = this.XamlRoot
                //                };


                //                var dr = ContentDialogResult.None;
                //                dr = await successDialog.ShowAsync();
                //                if (dialogResult == ContentDialogResult.Primary)
                //                {
                //                    isPaid = true;
                //                }
                //                else
                //                {
                //                    isPaid = false;
                //                }
                //            }
                //            else
                //            {
                //                // Hiển thị lỗi nếu số tiền không đủ
                //                ContentDialog errorDialog = new ContentDialog
                //                {
                //                    Title = "Insufficient Amount",
                //                    Content = "The received amount is less than the total amount.",
                //                    CloseButtonText = "OK",
                //                    XamlRoot = this.XamlRoot
                //                };

                //                await errorDialog.ShowAsync();
                //            }
                //        }
                //        else
                //        {
                //            // Hiển thị lỗi nếu nhập không hợp lệ
                //            ContentDialog errorDialog = new ContentDialog
                //            {
                //                Title = "Invalid Input",
                //                Content = "Please enter a valid amount.",
                //                CloseButtonText = "OK",
                //                XamlRoot = this.XamlRoot
                //            };

                //            await errorDialog.ShowAsync();
                //        }
                //    }
                //}

                var isSuccess = false;
                if (isPaid == true)
                {
                    // Gọi phương thức Checkout trong ViewModel để xử lý thanh toán
                    isSuccess = await ViewModel.Checkout();
                }

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
                        Title = "Thanh toán không thành công",
                        Content = "Thanh toán bị hủy hoặc xảy ra vấn đề.",
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
        // Kiểm tra xem hóa đơn đã thanh toán chưa
        if (ViewModel.IsPaid == true)
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
        // Không cho phép thay đổi nếu hóa đơn đã thanh toán
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
        // Cập nhật điểm tiêu dùng nếu số điện thoại khách hàng không rỗng
        if (ViewModel.Invoice.CustomerPhoneNumber != null && ViewModel.Invoice.CustomerPhoneNumber != "")
        {
            // Cập nhật điểm tiêu dùng
            int CurenPoints = App.GetService<IDao>().GetComsumedPoints(sender.Text);
            ViewModel.ConsumedPoints = (CurenPoints > ViewModel.Invoice.TotalPrice && ViewModel.Invoice.TotalPrice != 0) ? ViewModel.Invoice.TotalPrice : CurenPoints;
        }
    }


    private void CustomerPhoneNumber_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            // Sử dụng gợi ý đã chọn
            sender.Text = args.ChosenSuggestion.ToString();
            int CurenPoints = App.GetService<IDao>().GetComsumedPoints(sender.Text);
            ViewModel.ConsumedPoints = CurenPoints > ViewModel.Invoice.TotalPrice ? ViewModel.Invoice.TotalPrice : CurenPoints;
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
            // Kiểm tra điểm tiêu dùng và các món trong hóa đơn
            if (ViewModel.ConsumedPoints <= 0 || (ViewModel.Invoice.InvoiceItems == null || !ViewModel.Invoice.InvoiceItems.Any())) toggleSwitch.IsOn = false;
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
    private string GenerateUniqueOrderId()
    {
        return DateTime.Now.Ticks.ToString();
    }

    //
    private string OrderId
    {
        get; set;
    }

}

