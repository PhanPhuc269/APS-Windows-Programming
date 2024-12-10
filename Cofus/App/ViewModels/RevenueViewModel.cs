using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Model;

namespace App.ViewModels
{
    public class RevenueViewModel : ObservableRecipient
    {
        // Các thuộc tính hiện tại
        public int OrderCount
        {
            get; set;
        }
        public int TotalRevenue
        {
            get; set;
        }
        public int CashAmount
        {
            get; set;
        }

        public int TransferAmount => TotalRevenue - CashAmount;
        public string TotalRevenueFormatted => $"{TotalRevenue:N0} VND";
        public string CashAmountFormatted => $"{CashAmount:N0} VND";
        public string TransferAmountFormatted => $"{TransferAmount:N0} VND";

        public List<TopProduct> TopProducts { get; set; } = new();
        public List<TopCategory> TopCategories { get; set; } = new();
        public List<TopSeller> TopSellers { get; set; } = new();

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                LoadRevenueData(value);
            }
        }

        // Phương thức LoadRevenueData hiện tại
        public async Task LoadRevenueData(DateTime selectedDate)
        {
            var revenueData = await new MySqlDao().GetRevenue(selectedDate);
            var topProductsData = await new MySqlDao().GetTopProducts(selectedDate);
            var topCategoriesData = await new MySqlDao().GetTopCategories(selectedDate);
            var topSellersData = await new MySqlDao().GetTopSellers(selectedDate);

            OrderCount = revenueData.OrderCount;
            TotalRevenue = revenueData.TotalRevenue;
            CashAmount = revenueData.CashAmount;
            TopProducts = topProductsData;
            TopCategories = topCategoriesData;
            TopSellers = topSellersData;
        }
    }
}
