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

    private async Task<string> GenerateMoMoQRCodeAsync()
    {
        var invoice = ViewModel.Invoice;
        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));
        // Cấu hình thông tin MoMo
        var partnerCode = $"{Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE")}"; 
        var accessKey = $"{Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY")}";
        var secretKey = $"{Environment.GetEnvironmentVariable("MOMO_SECRET_KEY")}";
        var endpoint = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT")}"; // Sandbox URL

        // Thông tin thanh toán
        var orderId = GenerateUniqueOrderId();
        OrderId = orderId;
        var orderInfo = $"Thanh toán hóa đơn #{orderId}";
        var amount = invoice.AmountDue.ToString();
        var notifyUrl = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT_URL")}"; // URL nhận thông báo từ MoMo
        var returnUrl = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT_NOTICEURL")}"; // URL sau khi thanh toán thành công
        var requestId = Guid.NewGuid().ToString();
        var extraData = "";

        // Chuỗi cần ký
        var rawSignature = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";

        // Ký dữ liệu bằng HMAC SHA256
        string signature = GenerateSignature(rawSignature, secretKey);

        // Tạo dữ liệu JSON gửi đến MoMo
        var requestBody = new
        {
            partnerCode,
            accessKey,
            requestId,
            amount,
            orderId,
            orderInfo,
            redirectUrl = returnUrl,
            ipnUrl = notifyUrl,
            extraData,
            requestType = "captureWallet",
            signature
        };

        using System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        System.Net.Http.HttpResponseMessage response = await client.PostAsync(endpoint, content);


        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var paymentResponse = System.Text.Json.JsonSerializer.Deserialize<MoMoResponse>(responseContent);
            return paymentResponse?.qrCodeUrl; // Dùng URL trả về cho QR
        }


        return null;
    }
    private async Task<ContentDialog> ShowMoMoQRCode1()
    {
        try
        {
            // Initialize the MoMoPaymentProcessor
            var paymentProcessor = new MoMoPayment();
            var invoice = ViewModel.Invoice;
            DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));
            // Cấu hình thông tin MoMo
            var partnerCode = $"{Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE")}";
            var accessKey = $"{Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY")}";
            var secretKey = $"{Environment.GetEnvironmentVariable("MOMO_SECRET_KEY")}";
            var endpoint = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT")}"; // Sandbox URL

            // Thông tin thanh toán
            var orderId = GenerateUniqueOrderId();
            OrderId = orderId;
            var orderInfo = $"Thanh toán hóa đơn #{orderId}";
            var amount = invoice.AmountDue.ToString();
            var notifyUrl = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT_URL")}"; // URL nhận thông báo từ MoMo
            var returnUrl = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT_NOTICEURL")}"; // URL sau khi thanh toán thành công
            var requestId = Guid.NewGuid().ToString();
            var extraData = "";

            // Generate the MoMo QR code URL
            string requestUrl = await paymentProcessor.ProcessPaymentAsync(partnerCode, accessKey, secretKey, orderId, orderInfo, returnUrl, notifyUrl, requestId, amount, extraData);
            //string requestUrl = await paymentProcessor.ProcessPaymentAsync("MOMO", "F8BBA842ECF85", "K951B6PE1waDMi640xX08PD3vg6EkVlz", orderId, orderInfo, returnUrl, notifyUrl, requestId, amount, "");
            if (!string.IsNullOrEmpty(requestUrl))
            {
                return await OpenPaymentWebViewDialog(requestUrl);

            }
            else
            {
                Debug.WriteLine("Failed to generate QR code URL.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing MoMo QR code: {ex.Message}");
        }
        return null;
    }

    public async Task<WriteableBitmap> GenerateQRCodeAsync(string data)
    {
        try
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode byteQRCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeBytes = byteQRCode.GetGraphic(20);

            using (var ms = new MemoryStream(qrCodeBytes))
            {
                ms.Position = 0;
                var writeableBitmap = new WriteableBitmap(300, 300);
                await writeableBitmap.SetSourceAsync(ms.AsRandomAccessStream());
                return writeableBitmap;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error generating QR code: {ex.Message}");
            return null;
        }
    }

    public bool IsValidBitmapImage(BitmapImage bitmapImage)
    {
        try
        {
            // Attempt to access the pixel data to check if the image is valid
            using (MemoryStream ms = new MemoryStream())
            {
                bitmapImage.SetSource(ms.AsRandomAccessStream());
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            Debug.WriteLine($"Invalid BitmapImage: {ex.Message}");
            return false;
        }
    }

    private async Task ShowMoMoQRCode()
    {
        var qrCodeData = await GenerateMoMoQRCodeAsync();

        if (!string.IsNullOrEmpty(qrCodeData))
        {
            var writeableBitmap = await GenerateQRCodeAsync(qrCodeData);
            if (writeableBitmap != null)
            {
                var qrImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = writeableBitmap,
                    Width = 300,
                    Height = 300,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var qrDialog = new ContentDialog
                {
                    Title = "Please scan the QR code to pay",
                    Content = qrImage,
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
                await qrDialog.ShowAsync();
                _currentDialog = qrDialog;
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Unable to generate QR code.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
        else
        {
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = "Unable to generate QR code data.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private async Task<bool> CheckTransactionStatusAsync(string orderId)
    {
        var partnerCode = "MOMOBKUN20180529";
        var accessKey = "klm05TvNBzhg7h7j";
        var secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
        var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";

        var requestId = Guid.NewGuid().ToString();
        var rawSignature = $"accessKey={accessKey}&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}";

        // Ký dữ liệu
        string signature = GenerateSignature(rawSignature, secretKey);

        // Dữ liệu gửi đi
        var requestBody = new
        {
            partnerCode,
            accessKey,
            requestId,
            orderId,
            signature,
            requestType = "transactionStatus"
        };

        using System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        System.Net.Http.HttpResponseMessage response = await client.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var statusResponse = System.Text.Json.JsonSerializer.Deserialize<MoMoCallbackResponse>(responseContent);


            return statusResponse?.resultCode == 0; // 0 là thành công
        }

        return false;
    }
    private string GenerateSignature(string rawData, string secretKey)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }


    private CancellationTokenSource _cancellationTokenSource;

    private ContentDialog _currentDialog;

    private bool VerifyMoMoSignature(MoMoCallbackResponse callback)
    {
        var rawData = $"amount={callback.amount}&orderId={callback.orderId}&partnerCode={callback.partnerCode}&transId={callback.transId}";
        var secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa"; 
        var computedSignature = GenerateSignature(rawData, secretKey);
        return computedSignature == callback.signature;
    }
    
    async public Task<ContentDialog> ShowVNPayQRCode()
    {
        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));

        // Cấu hình thanh toán
        string vnp_TmnCode = $"{Environment.GetEnvironmentVariable("VNP_TMNCODE")}";  // Mã website VNPAY
        string secretKey = $"{Environment.GetEnvironmentVariable("SECRET_KEY")}";  // Chuỗi bí mật VNPAY
        string vnp_Url = $"{Environment.GetEnvironmentVariable("VNP_URL")}";  // URL thanh toán VNPAY
        string vnp_ReturnUrl = $"{Environment.GetEnvironmentVariable("VNP_RETURN_URL")}";  // URL trả về
        string vnp_IpAddr = $"{Environment.GetEnvironmentVariable("VNP_IP_ADDR")}";  // IP của khách hàng
        string vnp_TxnRef = GenerateUniqueOrderId();  // Mã giao dịch (duy nhất)
        string vnp_OrderInfo = $"Thanh toan hoa don {ViewModel.Invoice.InvoiceNumber}";  // Mô tả đơn hàng
        string vnp_OrderType = "billpayment";  // Loại đơn hàng
        int vnp_Amount = ViewModel.Invoice.AmountDue;  // Số tiền (VNĐ)


        // Ví dụ về request data
        var requestData = new Dictionary<string, string>
        {
            {"vnp_Amount", (vnp_Amount * 100).ToString() }, // Nhân 100 để chuyển về VND
            {"vnp_Command", "pay" },
            {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            {"vnp_CurrCode", "VND" },
            {"vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss")  },
            {"vnp_IpAddr", vnp_IpAddr },
            {"vnp_Locale", "vn" }, // Ngôn ngữ hiển thị: "vn" (Tiếng Việt), "en" (Tiếng Anh)
            {"vnp_OrderInfo", vnp_OrderInfo },
            {"vnp_OrderType", vnp_OrderType },
            {"vnp_ReturnUrl", vnp_ReturnUrl },
            {"vnp_TmnCode", vnp_TmnCode },
            {"vnp_TxnRef", vnp_TxnRef },
            {"vnp_Version", "2.1.1" },
        };

        // Chuỗi bí mật của bạn từ VNPAY
        string vnpHashSecret = $"{Environment.GetEnvironmentVariable("VNP_HASHSECRET")}";

        // Base URL của VNPAY
        string baseUrl = $"{Environment.GetEnvironmentVariable("VNP_URL")}";

        // Tạo URL thanh toán
        string requestUrl = CreateRequestUrl(baseUrl, vnpHashSecret, requestData);
        return await OpenPaymentWebViewDialog(requestUrl);

    }

    public async Task<ContentDialog> OpenPaymentWebViewDialog(string token, bool autoCheck=false)
    {
        string url = $"{token}";  // URL thanh toán

        // Khởi tạo WebView2
        WebView2 webView = new WebView2();

        // Đảm bảo WebView2 đã được khởi tạo
        await webView.EnsureCoreWebView2Async(null);

        // Chuyển hướng đến URL thanh toán
        webView.CoreWebView2.Navigate(url);

        // Tạo Grid và thêm WebView2 vào
        Grid grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch, // Stretch Grid horizontally
            VerticalAlignment = VerticalAlignment.Stretch, // Stretch Grid vertically
            MinHeight = 500, // Set minimum height for the Grid
            MinWidth = 500,  // Set minimum width for the Grid
            Height = double.NaN, // Allow grid to adjust its height based on WebView2
            Width = double.NaN // Allow grid to adjust its width based on WebView2
        };
        grid.Children.Add(webView);

        // Tạo ContentDialog và thêm Grid vào
        ContentDialog dialog = new ContentDialog
        {
            Title = "Payment",
            Content = grid,
            PrimaryButtonText = "Xác nhận đã thanh toán",
            CloseButtonText = "Hủy",
            XamlRoot = this.XamlRoot,
            Width = 900,  // Set the desired width of the dialog
            Height = 700  // Set the desired height of the dialog
        };
        dialog.IsPrimaryButtonEnabled = !autoCheck;

        return dialog;

    }

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret, Dictionary<string, string> requestData)
    {
        var data = new StringBuilder();

        // Duyệt qua tất cả các tham số yêu cầu và loại bỏ các tham số có giá trị null hoặc empty
        foreach (var (key, value) in requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            // Mã hóa URL cho các tham số và nối chúng lại
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        // Tạo query string từ các tham số đã mã hóa
        var querystring = data.ToString();

        // Thêm dấu ? vào baseUrl nếu querystring không rỗng
        baseUrl += "?" + querystring;

        // Loại bỏ dấu '&' cuối cùng trong query string
        if (querystring.Length > 0)
        {
            querystring = querystring.Remove(querystring.Length - 1, 1);
        }

        // Tính toán chữ ký (SecureHash) từ query string đã được mã hóa
        var vnpSecureHash = HmacSha512(vnpHashSecret, querystring);

        // Thêm chữ ký vào URL cuối cùng
        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

        return baseUrl;
    }

    private string HmacSha512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

   
}

