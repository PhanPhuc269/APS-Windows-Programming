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


namespace App.Views;

[AddINotifyPropertyChangedInterface]
public sealed partial class InvoiceControl : UserControl
{
    public InvoiceControlViewModel ViewModel { get; }

    public InvoiceControl()
    {
        this.InitializeComponent();
        ViewModel = new InvoiceControlViewModel();

        // Khởi động HTTP Listener để nhận callback từ MoMo
        StartCallbackListener();
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
                if(paymentMethod== "QR Code MoMo") await ShowMoMoQRCode();
                if(paymentMethod== "Cash") await ShowVNPayQRCode();


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
        var partnerCode = "MOMOBKUN20180529";
        var accessKey = "klm05TvNBzhg7h7j";
        var secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
        var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create"; // Sandbox URL

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

            //var diff = new MoMoPaymentProcessor();
            //string qrCodeUrl=await diff.ProcessPaymentAsync("MOMO", "F8BBA842ECF85", "K951B6PE1waDMi640xX08PD3vg6EkVlz", orderId, orderInfo, returnUrl, notifyUrl, requestId, amount, "");
            //var writeableBitmap = await diff.GenerateQRCodeAsync(qrCodeUrl);
            //if (writeableBitmap != null)
            //{
            //    var qrImage = new Microsoft.UI.Xaml.Controls.Image
            //    {
            //        Source = writeableBitmap,
            //        Width = 300,
            //        Height = 300,
            //        HorizontalAlignment = HorizontalAlignment.Center,
            //        VerticalAlignment = VerticalAlignment.Center
            //    };

            //    var qrDialog = new ContentDialog
            //    {
            //        Title = "Please scan the QR code to pay",
            //        Content = qrImage,
            //        CloseButtonText = "Close",
            //        XamlRoot = this.XamlRoot
            //    };
            //    await qrDialog.ShowAsync();
            //    _currentDialog = qrDialog;
            //}
            //else
            //{
            //    var errorDialog = new ContentDialog
            //    {
            //        Title = "Error",
            //        Content = "Unable to generate QR code.",
            //        CloseButtonText = "OK",
            //        XamlRoot = this.XamlRoot
            //    };
            //    await errorDialog.ShowAsync();
            //}
            // Trả về URL hoặc QR code để hiển thị
            return paymentResponse?.qrCodeUrl; // Dùng URL trả về cho QR
        }


        return null;
    }
    public class MoMoPaymentProcessor
    {
        private static readonly string apiUrl = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
        private string GenerateSignature(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<WriteableBitmap> GenerateQRCodeAsync(string data)
        {
            try
            {
                // Tạo đối tượng QRCodeGenerator
                QRCodeGenerator qrGenerator = new QRCodeGenerator();

                // Chuyển đổi dữ liệu thành QRCodeData
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);

                // Tạo đối tượng QRCode từ QRCodeData
                QRCode qrCode = new QRCode(qrCodeData);

                // Tạo ảnh QR code dưới dạng System.Drawing.Bitmap
                Bitmap qrCodeBitmap = qrCode.GetGraphic(20); // 20 là độ lớn các block trong mã QR

                // Chuyển đổi Bitmap thành byte[] (chuyển đổi Bitmap thành MemoryStream)
                using (var ms = new MemoryStream())
                {
                    qrCodeBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Lưu Bitmap vào MemoryStream dưới dạng PNG
                    byte[] qrCodeBytes = ms.ToArray(); // Lấy mảng byte từ MemoryStream

                    // Chuyển đổi byte[] thành WriteableBitmap
                    using (var stream = new MemoryStream(qrCodeBytes))
                    {
                        var writeableBitmap = new WriteableBitmap(300, 300); // Đặt kích thước phù hợp
                        await writeableBitmap.SetSourceAsync(stream.AsRandomAccessStream()); // Đặt nguồn cho WriteableBitmap
                        return writeableBitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating QR code: {ex.Message}");
                return null;
            }
        }


        public async Task<string> ProcessPaymentAsync(string partnerCode, string accessKey, string secretKey, string orderId, string orderInfo, string returnUrl, string notifyUrl, string requestId, string amount, string extraData)
        {
            // Tạo chuỗi cần ký
            var rawSignature = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData={extraData}";
            // Ký chuỗi bằng HMAC SHA256
            string signature = GenerateSignature(rawSignature, secretKey);

            // Dữ liệu thanh toán
            var paymentRequest = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount,
                orderId,
                orderInfo,
                returnUrl,
                notifyUrl,
                extraData,
                requestType = "captureMoMoWallet",
                signature
            };

            using (var client = new System.Net.Http.HttpClient())
            {
                var json = System.Text.Json.JsonSerializer.Serialize(paymentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentResponse = System.Text.Json.JsonSerializer.Deserialize<dynamic>(responseContent);

                // Extract the qrCodeUrl from the JSON response
                if (paymentResponse.TryGetProperty("qrCodeUrl", out JsonElement qrCodeUrlElement))
                {
                    return qrCodeUrlElement.GetString();
                }
                return null;
            }
        }
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
    private string SaveQRCodeToFile(string data, string filePath)
    {
        // Tạo QR code từ chuỗi dữ liệu
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        BitmapByteQRCode byteQRCode = new BitmapByteQRCode(qrCodeData);

        // Lấy hình ảnh QR code dưới dạng byte[]
        byte[] qrCodeBytes = byteQRCode.GetGraphic(20);

        // Lưu byte[] thành file
        File.WriteAllBytes(filePath, qrCodeBytes);

        return filePath; // Trả về đường dẫn file đã lưu
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


    private HttpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private void StartCallbackListener()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:5000/callback/");
        _cancellationTokenSource = new CancellationTokenSource();
        _listener.Start();

        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    await HandleCallbackAsync(context);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Listener Error: {ex.Message}");
                }
            }
        });
    }

    private async Task HandleCallbackAsync(HttpListenerContext context)
    {
        var request = context.Request;

        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            var callbackData = await reader.ReadToEndAsync();
            Debug.WriteLine($"MoMo Callback Data: {callbackData}");

            // Xử lý dữ liệu callback
            var isTransactionSuccessful = await ProcessMoMoCallbackAsync(callbackData);

            // Gửi phản hồi HTTP 200 OK cho MoMo
            var response = context.Response;
            var responseString = isTransactionSuccessful ? "SUCCESS" : "FAILED";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
    private ContentDialog _currentDialog;
    private async Task<bool> ProcessMoMoCallbackAsync(string callbackData)
    {
        try
        {
            // Phân tích JSON từ MoMo
            var callbackJson = System.Text.Json.JsonSerializer.Deserialize<MoMoCallbackResponse>(callbackData);

            // Kiểm tra chữ ký hợp lệ
            //if (!VerifyMoMoSignature(callbackJson))
            //{
            //    Debug.WriteLine("Invalid MoMo Signature");
            //    return false;
            //}

            // Cập nhật trạng thái giao dịch dựa trên `resultCode`
            if (callbackJson.resultCode == 0)
            {
                Debug.WriteLine($"Transaction Success for OrderId: {callbackJson.orderId}");
                // Gọi phương thức Checkout trong ViewModel để xử lý thanh toán
                var isSuccess = await ViewModel.Checkout();

                if (isSuccess)
                {
                    //Đóng dialog hiện tại nếu có


                    // Thông báo khi thanh toán thành công
                    var dialog = new ContentDialog
                    {
                        Title = "Thanh toán thành công",
                        Content = $"Hóa đơn của bạn đã được lưu. Phương thức thanh toán: {ViewModel.Invoice.PaymentMethod}.",
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
                return true;
            }
            else
            {
                Debug.WriteLine($"Transaction Failed for OrderId: {callbackJson.orderId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing callback: {ex.Message}");
            return false;
        }
    }

    private bool VerifyMoMoSignature(MoMoCallbackResponse callback)
    {
        var rawData = $"amount={callback.amount}&orderId={callback.orderId}&partnerCode={callback.partnerCode}&transId={callback.transId}";
        var secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa"; 
        var computedSignature = GenerateSignature(rawData, secretKey);
        return computedSignature == callback.signature;
    }
    //private async Task ShowVNPayQRCode()
    //{
    //    string terminalId = "EOASESC4"; // Mã Merchant (Terminal ID)
    //    string secretKey = "6VN91W7ILCKUGFYYOQUEL2DWL37K8SV5"; // Secret Key
    //    string orderId = GenerateUniqueOrderId(); // Tạo ID đơn hàng
    //    string orderInfo = "Payment for Invoice"; // Thông tin đơn hàng
    //    string amount = ViewModel.Invoice.AmountDue.ToString(); // Số tiền thanh toán
    //    string returnUrl = "https://your-website.com/return"; // Địa chỉ trả về sau khi thanh toán
    //    string cancelUrl = "https://your-website.com/cancel"; // Địa chỉ hủy thanh toán
    //    string ipAddress = "127.0.0.1"; // Địa chỉ IP người dùng

    //    VNPayPaymentProcessor paymentProcessor = new VNPayPaymentProcessor();
    //    string response = await paymentProcessor.CreatePaymentAsync(terminalId, orderId, orderInfo, amount, returnUrl, cancelUrl, ipAddress, secretKey);


    //    // Phân tích phản hồi từ VNPay để lấy URL QR code
    //     var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

    //    string qrCodeUrl = responseObject["qr_code_url"];

    //    // Tạo mã QR
    //    WriteableBitmap writeableBitmap = await GenerateQRCodeAsync(qrCodeUrl);

    //    if (writeableBitmap != null)
    //    {
    //        var qrImage = new Microsoft.UI.Xaml.Controls.Image
    //        {
    //            Source = writeableBitmap,
    //            Width = 300,
    //            Height = 300,
    //            HorizontalAlignment = HorizontalAlignment.Center,
    //            VerticalAlignment = VerticalAlignment.Center
    //        };

    //        var qrDialog = new ContentDialog
    //        {
    //            Title = "Please scan the QR code to pay",
    //            Content = qrImage,
    //            CloseButtonText = "Close",
    //            XamlRoot = this.XamlRoot
    //        };
    //        await qrDialog.ShowAsync();
    //        _currentDialog = qrDialog;
    //    }
    //    else
    //    {
    //        var errorDialog = new ContentDialog
    //        {
    //            Title = "Error",
    //            Content = "Unable to generate QR code.",
    //            CloseButtonText = "OK",
    //            XamlRoot = this.XamlRoot
    //        };
    //        await errorDialog.ShowAsync();
    //    }
    //}

    //class VNPayPaymentProcessor
    //{
    //    private static readonly string apiUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"; // URL của VNPay (sử dụng sandbox cho môi trường thử nghiệm)

    //    public async Task<string> CreatePaymentAsync(string terminalId, string orderId, string orderInfo, string amount, string returnUrl, string cancelUrl, string ipAddress, string secretKey)
    //    {
    //        string txnTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // Thời gian giao dịch

    //        // Tạo chuỗi dữ liệu yêu cầu

    //        // Sinh chữ ký bảo mật
    //        string data = $"vnp_Amount={amount}&vnp_Command=pay&vnp_CreateDate={txnTime}&vnp_CurrCode=VND&vnp_IpAddr={ipAddress}&vnp_Locale=vn&vnp_OrderInfo={orderInfo}&vnp_OrderType=other&vnp_ReturnUrl={returnUrl}&vnp_TmnCode={terminalId}&vnp_TxnRef={orderId}&vnp_Version=2.1.0";
    //        string signature = GenerateSignature(data, secretKey);

    //        // Tạo yêu cầu thanh toán
    //        var paymentRequest = new
    //        {
    //            vnp_Amount = amount,
    //            vnp_Command = "pay",
    //            vnp_CreateDate = txnTime,
    //            vnp_CurrCode = "VND",
    //            vnp_IpAddr = "127.0.0.1",
    //            vnp_Locale = "vn",
    //            vnp_OrderInfo = "thanh",
    //            vnp_OrderType = "other",
    //            vnp_ReturnUrl = "https://domainmerchant.vn/ReturnUrl",
    //            vnp_TmnCode = terminalId,
    //            vnp_TxnRef = orderId,
    //            vnp_Version = "2.1.0",
    //            vnp_SecureHash = signature
    //        };

    //        // Chuyển yêu cầu thành chuỗi JSON
    //        string jsonRequest = JsonConvert.SerializeObject(paymentRequest);

    //        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
    //        {
    //            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
    //            System.Net.Http.HttpResponseMessage response = await client.PostAsync(apiUrl, content);

    //            if (response.IsSuccessStatusCode)
    //            {
    //                string jsonResponse = await response.Content.ReadAsStringAsync();

    //                return jsonResponse;
    //            }
    //            else
    //            {
    //                throw new Exception("Payment processing failed.");
    //            }
    //        }
    //    }

    //    // Hàm tạo chữ ký bảo mật (HMACSHA256)
    //    private string GenerateSignature(string data, string secretKey)
    //    {
    //        using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
    //        {
    //            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    //            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
    //        }
    //    }

    //}
    async public Task ShowVNPayQRCode()
    {
        // Cấu hình thanh toán
        string vnp_TmnCode = "EOASESC4";  // Mã website VNPAY
        string secretKey = "PHOSRMU09ADV8IIETWZS2KEFKUA7ELKQ";  // Chuỗi bí mật VNPAY
        string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";  // URL thanh toán VNPAY
        string vnp_ReturnUrl = "https://sandbox.vnpayment.vn/tryitnow/Home/VnPayReturn";  // URL trả về
        string vnp_IpAddr = "127.0.0.1";  // IP của khách hàng
        string vnp_TxnRef = GenerateUniqueOrderId();  // Mã giao dịch (duy nhất)
        string vnp_OrderInfo = "Thanh toan";  // Mô tả đơn hàng
        string vnp_OrderType = "billpayment";  // Loại đơn hàng
        int vnp_Amount = 30000;  // Số tiền (VNĐ)


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
        string vnpHashSecret = "6VN91W7ILCKUGFYYOQUEL2DWL37K8SV5";

        // Base URL của VNPAY
        string baseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        // Tạo URL thanh toán
        string requestUrl = CreateRequestUrl(baseUrl, vnpHashSecret, requestData);
        await OpenPaymentWebViewDialog(requestUrl);

    }

    public async Task OpenPaymentWebViewDialog(string token)
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
            CloseButtonText = "Close",
            XamlRoot = this.XamlRoot,
            Width = 900,  // Set the desired width of the dialog
            Height = 700  // Set the desired height of the dialog
        };

        // Hiển thị ContentDialog
        await dialog.ShowAsync();
    }

    public async Task<string> GetTokenFromVnpay(string requestUrl)
    {
        // Create an instance of HttpClientHandler
        HttpClientHandler handler = new HttpClientHandler
        {
            AllowAutoRedirect = true // Set AllowAutoRedirect to true
        };

        // Create an instance of HttpClient with the handler
        System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler);

        // Create a HttpRequestMessage
        var requestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, requestUrl);

        // Send the request and get the response
        System.Net.Http.HttpResponseMessage response = await client.SendAsync(requestMessage);

        // Check the status of the response
        if (response.IsSuccessStatusCode)
        {
            // Get the final URL after redirection
            Uri finalUrl = response.RequestMessage.RequestUri;
            string token = GetTokenFromUrl(finalUrl.ToString());

            return token;
        }

        // If there is an error sending the request, return null
        return null;
    }


    // Phương thức trích xuất token từ URL (ví dụ: từ "https://sandbox.vnpayment.vn/paymentv2/Transaction/PaymentMethod.html?token=b335ad60c1164de8b011da4b5ac2d825")
    private string GetTokenFromUrl(string url)
    {
        var uri = new Uri(url);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return queryParams["token"];
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

