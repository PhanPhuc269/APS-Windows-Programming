﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using QRCoder;
using System.Net;
using App.Model;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Controls;

namespace App;

public class VietQRPayment:IPaymentMethod
{
    private readonly string _bankId = "MB"; // ID ngân hàng
    private readonly string _accountNo = "0363353610"; // Số tài khoản
    private readonly string _accountName = "PHAN HONG PHUC"; // Tên chủ tài khoản
    private readonly string _template = "full"; // Template QR (có thể thay đổi)
    private bool _isSuccess = false;

    public async Task<bool> ProcessPayment(Invoice invoice)
    {
        var token = GenerateUniqueID.GenerateUniqueOrderID();
        var qrUrl = GenerateVietQR(invoice, token);
        ContentDialog checkoutDia = await OpenDialog.OpenPaymentWebViewDialog(qrUrl,true);
        checkoutDia.XamlRoot = App.MainWindow.Content.XamlRoot;
        var dialogResult = checkoutDia.ShowAsync();
        // Kiểm tra thanh toán sau mỗi 5 giây cho đến khi thanh toán thành công
        bool isPaymentSuccessful = await WaitForPaymentAsync(invoice, token);
        if (isPaymentSuccessful == true) 
        {
            checkoutDia.Hide();
            return true; 
        }

        var result=await dialogResult;
        return result == ContentDialogResult.Primary;
    }

    // Tạo mã QR thanh toán VietQR

    public async Task<ContentDialog> ShowMoMoQRCode(Invoice invoice)
    {
        try
        {
            var token = GenerateUniqueID.GenerateUniqueOrderID(); // Tạo mã đơn hàng duy nhất
            var qrUrl = GenerateVietQR(invoice, token); // Tạo URL mã QR

            // Hiển thị ContentDialog chứa mã QR
            return await OpenDialog.OpenPaymentWebViewDialog(qrUrl, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi tạo mã QR: " + ex.Message);
            return null;
        }
    }
    public string GenerateVietQR(Invoice invoice, string token)
    {
        decimal price = invoice.AmountDue;
        var invoiceInfo = $"Invoice #{token}";
        var qrUrl = $"https://img.vietqr.io/image/{_bankId}-{_accountNo}-{_template}.png?amount={price}&addInfo={Uri.EscapeDataString(invoiceInfo)}&accountName={Uri.EscapeDataString(_accountName)}";
        return qrUrl;
    }

    // Kiểm tra thanh toán từ Google Sheets
    public async Task<bool> CheckPaymentAsync(Invoice invoice, string token)
    {
        if (_isSuccess)
        {
            return true; // Nếu đã thành công, trả về true ngay lập tức
        }

        var url = "https://script.google.com/macros/s/AKfycbyQdQ6RRVmYOv80xyZOXvhE5w_eeBisPE8_aDgpgKXW6wHZmXq6CA9BBiYO0bvJfcWl/exec"; // Thay đổi URL theo API của bạn
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<ApiResponse>(response);

                decimal price = invoice.AmountDue;
                string content = $"Invoice {token}";

                foreach (var transaction in data.Data)
                {
                    if (transaction.GiaTri >= price && RemoveVietnameseTones(transaction.MoTa).Contains(RemoveVietnameseTones(content)))
                    {
                        _isSuccess = true;
                        return true; // Thanh toán thành công
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi kiểm tra thanh toán: " + ex.Message);
            }
        }

        return false; // Thanh toán không thành công
    }

    // Hàm để loại bỏ dấu tiếng Việt
    private string RemoveVietnameseTones(string str)
    {
        return str
            .Normalize(System.Text.NormalizationForm.FormD)
            .Replace("đ", "d")
            .Replace("Đ", "D")
            .Replace(@"\p{Mn}", "");
    }

    public class ApiResponse
    {
        [JsonPropertyName("data")]
        public Transaction[] Data
        {
            get; set;
        }

        [JsonPropertyName("error")]
        public bool Error
        {
            get; set;
        }
    }

    public class Transaction
    {
        [JsonPropertyName("Mã GD")]      // Ánh xạ trường JSON "Mã GD" vào thuộc tính "MaGD" trong C#
        public long MaGD
        {
            get; set;
        }    // Dùng tên không dấu

        [JsonPropertyName("Mô tả")]      // Ánh xạ trường JSON "Mô tả" vào thuộc tính "MoTa" trong C#
        public string MoTa
        {
            get; set;
        }  // Dùng tên không dấu

        [JsonPropertyName("Giá trị")]    // Ánh xạ trường JSON "Giá trị" vào thuộc tính "GiaTri" trong C#
        public decimal GiaTri
        {
            get; set;
        } // Dùng tên không dấu

        [JsonPropertyName("Ngày diễn ra")] // Ánh xạ trường JSON "Ngày diễn ra" vào thuộc tính "NgayDienRa" trong C#
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime NgayDienRa
        {
            get; set;
        } // Dùng tên không dấu

        [JsonPropertyName("Số tài khoản")] // Ánh xạ trường JSON "Số tài khoản" vào thuộc tính "SoTaiKhoan" trong C#
        public string SoTaiKhoan
        {
            get; set;
        } // Dùng tên không dấu
    }
    // Hàm để kiểm tra thanh toán liên tục cho đến khi thành công
    public async Task<bool> WaitForPaymentAsync(Invoice invoice, string token, int delayMilliseconds = 3000)
    {
        // Kiểm tra thanh toán cho đến khi thành công hoặc hết số lần thử
        while (!await CheckPaymentAsync(invoice, token))
        {
            await Task.Delay(delayMilliseconds); // Chờ một khoảng thời gian trước khi kiểm tra lại
        }

        // Nếu thanh toán thành công
        return true;
    }
}
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string[] _formats = new[]
    {
            "yyyy-MM-ddTHH:mm:ssZ",
            "dd/MM/yyyy HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss" // Add this format
        };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string dateString = reader.GetString();
        foreach (var format in _formats)
        {
            if (DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
        }
        throw new JsonException($"Unable to convert \"{dateString}\" to DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_formats[0])); // Use the first format for writing
    }
}
