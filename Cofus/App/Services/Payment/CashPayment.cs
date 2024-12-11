using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using App.Model;

namespace App.Services.Payment;
public class CashPayment : IPaymentMethod
{
    public async Task<bool> ProcessPayment(Invoice invoice)
    {
        TextBox receivedAmountTextBox = new TextBox { PlaceholderText = "Amount" };
        TextBlock errorTextBlock = new TextBlock { Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red), Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };

        ContentDialog cashDialog = new ContentDialog
        {
            Title = "Cash Payment",
            Content = new StackPanel
            {
                Children =
                    {
                        new TextBlock { Text = "Nhập số tiền nhận từ khách:" },
                        receivedAmountTextBox,
                        errorTextBlock
                    }
            },
            PrimaryButtonText = "Xác nhận",
            CloseButtonText = "Hủy",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        while (true)
        {
            var dialogResult = await cashDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary)
            {
                if (decimal.TryParse(receivedAmountTextBox.Text, out decimal receivedAmount))
                {
                    decimal totalAmount = invoice.TotalPrice;
                    decimal change = receivedAmount - totalAmount;

                    if (change >= 0)
                    {
                        // Mark invoice as paid
                        invoice.MarkAsPaid();

                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "Thanh toán thành công",
                            Content = $"Số tiền cần trả lại: {change:C}",
                            CloseButtonText = "OK",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        };

                        await successDialog.ShowAsync();
                        return true;
                    }
                    else
                    {
                        errorTextBlock.Text = "Số tiền nhận được ít hơn số tiền cần thanh toán.";
                        errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    }
                }
                else
                {
                    errorTextBlock.Text = "Please enter a valid amount.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
