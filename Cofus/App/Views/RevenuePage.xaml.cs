using App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;

namespace App.Views
{
    public sealed partial class RevenuePage : Page
    {
        public RevenueViewModel ViewModel
        {
            get; set;
        }

        public RevenuePage()
        {
            this.InitializeComponent();
            ViewModel = new RevenueViewModel();
            this.DataContext = ViewModel;

            // Load data asynchronously when the page is loaded
            LoadData();
        }

        private async void LoadData()
        {
            // Chỉ gọi một phương thức LoadRevenueData để tải tất cả dữ liệu
            await ViewModel.LoadRevenueData(DateTime.Today);
        }
    }
}
