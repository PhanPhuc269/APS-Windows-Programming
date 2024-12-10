using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Services.Maps;

namespace App.Views;

public sealed partial class ProductInputDialog : ContentDialog
{
    public ProductInputDialog(int beverageId)
    {
        this.InitializeComponent();
        BeverageId = beverageId;
    }

    public int BeverageId
    {
        get;
    }

    public int Quantity
    {
        get
        {
            int.TryParse(quantityTextBox.Text, out int quantity);
            return quantity;
        }
    }

    public string Notes
    {
        get => notesTextBox.Text;
    }

    public string SelectedSize
    {
        get
        {
            var selectedItem = sizeComboBox.SelectedItem as ComboBoxItem;
            return selectedItem?.Content.ToString();
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (true)
        {
            // Kiểm tra số lượng có hợp lệ không
            if (!int.TryParse(quantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                args.Cancel = true; // Ngăn không cho đóng dialog
                Notice.Text = "Vui lòng nhập số lượng hợp lệ";
                return;
            }

            int beverageSizeId = App.GetService<IDao>().GetBeverageSizeId(BeverageId, SelectedSize);

            // Kiểm tra nguyên liệu có đủ không
            int maxAvailableQuantity = await App.GetService<IDao>().GetMaxAvailableQuantityAsync(beverageSizeId);

            if (quantity > maxAvailableQuantity)
            {
                args.Cancel = true; // Ngăn không cho đóng dialog
                Notice.Text = $"Chỉ còn tối đa là {maxAvailableQuantity}.";
                quantityTextBox.Text = string.Empty; // Reset the quantity input
                return;
            }

            // Nếu đã nhập số lượng hợp lệ, thoát khỏi vòng lặp
            Notice.Text = string.Empty; // Clear any previous notice
            break;
        }
    }


    private void QuantityTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        // Chỉ cho phép nhập số
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }
}
