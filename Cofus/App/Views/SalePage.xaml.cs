using System;
using System.Collections.Generic;
using System.IO;
using App.Model;
using App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Devices.Enumeration;
using App.Views;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;


namespace App.Views;

public sealed partial class SalePage : Page
{
    public SalePageViewModel ViewModel
    {
        get;
    }
    public SalePage()
    {
        this.InitializeComponent();
        ViewModel = new SalePageViewModel();
        ViewCategory_Click(null, null);
    }

    public void TabView_Loaded(object sender, RoutedEventArgs e)
    {
        (sender as TabView).TabItems.Add(CreateNewTab((sender as TabView).TabItems.Count));
    }

    public void TabView_AddButtonClick(TabView sender, object args)
    {
        TabViewItem newInvoice = CreateNewTab(sender.TabItems.Count);
        sender.TabItems.Add(newInvoice);
        sender.SelectedItem = newInvoice;
    }

    public void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);
    }

    public TabViewItem CreateNewTab(int index)
    {
        // Tạo Tab mới cho hóa đơn
        TabViewItem newTab = new TabViewItem
        {
            Header = $"Hóa đơn",
            Content = new InvoiceControl(),
            Background = (SolidColorBrush)Application.Current.Resources["TabViewItemHeaderBackground"],
            Foreground = (SolidColorBrush)Application.Current.Resources["TabViewItemHeaderForeground"]
        };
        return newTab;
    }

    private void ViewCategory_Click(object sender, RoutedEventArgs e)
    {
        ContentCenter.Children.Clear();
        CategoryControl categoryView = new CategoryControl(this);
        ContentCenter.Children.Add(categoryView);
    }
    private void PendingOrder_Click(object sender, RoutedEventArgs e)
    {
        ContentCenter.Children.Clear();
        PendingControl pendingControl = new PendingControl();
        ContentCenter.Children.Add(pendingControl);
    }
    private void OnCategoryTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is TextBlock textBlock)
        {
            // Lấy giá trị của thẻ Tag để xác định danh mục được chọn
            string selectedCategory = textBlock.Tag.ToString();

            // Gọi hàm truy vấn cơ sở dữ liệu với danh mục đã chọn
            QueryDatabase(selectedCategory);
        }
    }

    private void QueryDatabase(string category)
    {
        // Thực hiện truy vấn lên cơ sở dữ liệu với tham số là category
        // Ví dụ: lấy các sản phẩm thuộc danh mục này
        // Kết nối và truy vấn DB, xử lý logic hiển thị dữ liệu

        // Gỉa sử ở đây bạn có hàm ExecuteQuery để gọi DB
        //var result = ExecuteQuery($"SELECT * FROM Products WHERE Category = '{category}'");

        // Hiển thị hoặc xử lý kết quả truy vấn
        //DisplayResults(result);
    }
    private void Control2_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Thêm logic xử lý sự kiện TextChanged ở đây
    }

    private void Control2_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        // Thêm logic xử lý sự kiện QuerySubmitted ở đây
    }

    private void Control2_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        // Thêm logic xử lý sự kiện SuggestionChosen ở đây
    }

}
