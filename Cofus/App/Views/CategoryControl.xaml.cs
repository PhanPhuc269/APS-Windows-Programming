using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using App.ViewModels;
using App.Model;
using App.Views;
using PropertyChanged;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App.Views;
[AddINotifyPropertyChangedInterface]
public sealed partial class CategoryControl : UserControl
{
    public CategoryViewModel ViewModel { get; set; }
    private SalePage _salePage;

    public CategoryControl(SalePage salePage)
    {
        this.InitializeComponent();
        
        ViewModel = new CategoryViewModel();
        _salePage = salePage;
    }

    private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Product product)
        {
            // Tạo ProductInputDialog mới
            var dialog = new ProductInputDialog(product.Id);
            dialog.XamlRoot = this.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (dialog.Quantity>0)
                {
                    var note = dialog.Notes;
                    var invoiceItem = new InvoiceItem
                    {
                        BeverageId = product.Id,
                        Size = dialog.SelectedSize,
                        Name = product.Name,
                        Price = App.GetService<IDao>().GetProductPrice(product.Id, dialog.SelectedSize),
                        Quantity = dialog.Quantity,
                        Note = !string.IsNullOrEmpty(note) ? note : "Không có ghi chú",
                    };

                    // Tìm InvoiceTabView bên trong SalePage
                    var invoiceTabView = _salePage.FindName("InvoiceTabView") as TabView;
                    if (invoiceTabView != null)
                    {
                        var selectedTab = invoiceTabView.SelectedItem as TabViewItem;
                        if (selectedTab != null)
                        {
                            // Tìm InvoiceControl tương ứng
                            var invoiceControl = selectedTab.Content as InvoiceControl;
                            if (invoiceControl != null && invoiceControl.ViewModel.IsPaid==false)
                            {
                                // Thêm sản phẩm vào hóa đơn
                                invoiceControl.ViewModel.AddInvoiceItem(invoiceItem);
                            }
                            else
                            {
                                Debug.WriteLine("Không tìm thấy InvoiceControl trong tab được chọn.");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Không tìm thấy InvoiceTabView trong SalePage.");
                    }
                }
            }
        }
    }

}
