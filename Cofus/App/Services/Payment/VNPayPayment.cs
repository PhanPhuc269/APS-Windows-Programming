using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using App.Model;

namespace App.Services.Payment;
public class VNPayPayment:IPaymentMethod
{
    public async Task<bool> ProcessPayment(Invoice invoice)
    {
        ContentDialog checkoutDia = await ShowVNPayQRCode(invoice);
        checkoutDia.XamlRoot = App.MainWindow.Content.XamlRoot;
        var dialogResult = await checkoutDia.ShowAsync();
        return dialogResult == ContentDialogResult.Primary;
    }

    public static async Task<ContentDialog> ShowVNPayQRCode(Invoice invoice)
    {
        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));

        // Cấu hình thanh toán
        string vnp_TmnCode = $"{Environment.GetEnvironmentVariable("VNP_TMNCODE")}";  // Mã website VNPAY
        string secretKey = $"{Environment.GetEnvironmentVariable("SECRET_KEY")}";  // Chuỗi bí mật VNPAY
        string vnp_Url = $"{Environment.GetEnvironmentVariable("VNP_URL")}";  // URL thanh toán VNPAY
        string vnp_ReturnUrl = $"{Environment.GetEnvironmentVariable("VNP_RETURN_URL")}";  // URL trả về
        string vnp_IpAddr = $"{Environment.GetEnvironmentVariable("VNP_IP_ADDR")}";  // IP của khách hàng
        string vnp_TxnRef = GenerateUniqueID.GenerateUniqueOrderID();  // Mã giao dịch (duy nhất)
        string vnp_OrderInfo = $"Thanh toan hoa don {invoice.InvoiceNumber}";  // Mô tả đơn hàng
        string vnp_OrderType = "billpayment";  // Loại đơn hàng
        int vnp_Amount = invoice.AmountDue;  // Số tiền (VNĐ)


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
        return await OpenDialog.OpenPaymentWebViewDialog(requestUrl);

    }

    public static string CreateRequestUrl(string baseUrl, string vnpHashSecret, Dictionary<string, string> requestData)
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

    private static string HmacSha512(string key, string inputData)
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
