using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace App;
public class OpenDialog
{

    public static async Task<ContentDialog> OpenPaymentWebViewDialog(string token, bool autoCheck = false)
    {
        var url = $"{token}";  // URL thanh toán

        // Khởi tạo WebView2
        var webView = new WebView2();

        // Đảm bảo WebView2 đã được khởi tạo
        await webView.EnsureCoreWebView2Async(null);

        // Chuyển hướng đến URL thanh toán
        webView.CoreWebView2.Navigate(url);

        // Tạo Grid và thêm WebView2 vào
        var grid = new Grid
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
        var dialog = new ContentDialog
        {
            Title = "Payment",
            Content = grid,
            PrimaryButtonText = "Xác nhận đã thanh toán",
            CloseButtonText = "Hủy",
            Width = 900,  // Set the desired width of the dialog
            Height = 700  // Set the desired height of the dialog
        };
        dialog.IsPrimaryButtonEnabled = !autoCheck;

        return dialog;

    }
}
