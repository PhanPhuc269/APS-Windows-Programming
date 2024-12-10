using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using App.Helpers;
using App.Model;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;

namespace App;

public class MoMoPayment
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
            if (paymentResponse.TryGetProperty("payUrl", out JsonElement payUrlElement))
            {
                return payUrlElement.GetString();
            }
            return null;
        }
    }
    public static async Task<ContentDialog> ShowMoMoQRCode(Invoice invoice)
    {
        try
        {
            // Initialize the MoMoPaymentProcessor
            var paymentProcessor = new MoMoPayment();
            DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));
            // Cấu hình thông tin MoMo
            var partnerCode = $"{Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE")}";
            var accessKey = $"{Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY")}";
            var secretKey = $"{Environment.GetEnvironmentVariable("MOMO_SECRET_KEY")}";
            var endpoint = $"{Environment.GetEnvironmentVariable("MOMO_ENDPOINT")}"; // Sandbox URL

            // Thông tin thanh toán
            var orderId = GenerateUniqueID.GenerateUniqueOrderID();
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
                return await OpenDialog.OpenPaymentWebViewDialog(requestUrl);

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
}