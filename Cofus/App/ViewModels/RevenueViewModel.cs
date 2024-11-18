using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Model;

namespace App.ViewModels
{
    public class RevenueViewModel : ObservableRecipient
    {
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

        public async Task LoadRevenueData(DateTime selectedDate)
        {
            var previousDate = selectedDate.AddDays(-1);

            var revenueData = await new MockDao().GetRevenue(selectedDate, previousDate);
            var topProductsData = await new MockDao().GetTopProducts(selectedDate);
            var topCategoriesData = await new MockDao().GetTopCategories(selectedDate);
            var topSellersData = await new MockDao().GetTopSellers(selectedDate);

            OrderCount = revenueData.OrderCount;
            TotalRevenue = revenueData.TotalRevenue;
            CashAmount = revenueData.CashAmount;
            TopProducts = topProductsData;
            TopCategories = topCategoriesData;
            TopSellers = topSellersData;
        }
    }
}
