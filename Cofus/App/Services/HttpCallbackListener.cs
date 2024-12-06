using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;

public class HttpCallbackListener
{
    private HttpListener _listener;
    private Thread _listenerThread;

    public void Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:5000/callback"); // Địa chỉ bạn muốn Momo gọi tới
        _listener.Start();
        Console.WriteLine("Listening for Momo callbacks...");

        // Chạy HttpListener trên một luồng riêng biệt để không làm nghẽn luồng chính
        _listenerThread = new Thread(ListenForRequests);
        _listenerThread.Start();
    }

    private void ListenForRequests()
    {
        while (_listener.IsListening)
        {
            try
            {
                var context = _listener.GetContext();
                var request = context.Request;

                using (var reader = new StreamReader(request.InputStream))
                {
                    // Đọc dữ liệu POST từ Momo
                    var callbackData = reader.ReadToEnd();
                    Console.WriteLine($"Received callback data: {callbackData}");

                    // Xử lý dữ liệu callback
                    HandleCallback(callbackData);
                }

                // Gửi phản hồi HTTP 200 OK để xác nhận đã nhận dữ liệu
                var response = context.Response;
                var responseString = "Received";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling callback: {ex.Message}");
            }
        }
    }

    public void HandleCallback(string callbackData)
    {
        try
        {
            // Phân tích JSON
            var callbackJson = JsonDocument.Parse(callbackData);
            var resultCode = callbackJson.RootElement.GetProperty("resultCode").GetInt32();
            var orderId = callbackJson.RootElement.GetProperty("orderId").GetString();
            var signature = callbackJson.RootElement.GetProperty("signature").GetString();

            // Kiểm tra giao dịch thành công
            if (resultCode == 0)
            {
                Console.WriteLine($"Transaction Successful for Order ID: {orderId}");
                // TODO: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu
            }
            else
            {
                Console.WriteLine($"Transaction Failed for Order ID: {orderId}");
                // TODO: Xử lý giao dịch thất bại
            }

            // Xác minh chữ ký (signature)
            if (!VerifySignature(callbackJson.RootElement))
            {
                Console.WriteLine("Invalid signature. Possible tampering detected!");
                return;
            }

            Console.WriteLine("Callback data processed successfully!");
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Error parsing JSON: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling callback: {ex.Message}");
        }
    }

    private bool VerifySignature(JsonElement callbackData)
    {
        string rawData = $"amount={callbackData.GetProperty("amount")}&orderId={callbackData.GetProperty("orderId")}";
        string secretKey = "YOUR_SECRET_KEY"; // Thay bằng secret key của bạn từ Momo
        string computedSignature = ComputeHmacSha256(rawData, secretKey);
        string receivedSignature = callbackData.GetProperty("signature").GetString();

        return computedSignature == receivedSignature;
    }

    private string ComputeHmacSha256(string rawData, string secretKey)
    {
        using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        }
    }

    public void Stop()
    {
        _listener.Stop();
        _listenerThread.Join(); // Đợi cho đến khi luồng listener kết thúc
    }
}
