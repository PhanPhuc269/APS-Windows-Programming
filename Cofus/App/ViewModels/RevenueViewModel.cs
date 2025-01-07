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

        // Thuộc tính ngày bắt đầu và ngày kết thúc
        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    OnPropertyChanged(nameof(StartDate)); // Đảm bảo kích hoạt
                }
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    OnPropertyChanged(nameof(EndDate)); // Đảm bảo kích hoạt
                }
            }
        }


        // Phương thức LoadRevenueData theo khoảng thời gian
        public async Task LoadRevenueData(DateTime startDate, DateTime endDate)
        {
            var revenueData = await new MySqlDao().GetRevenue(startDate, endDate);
            var topProductsData = await new MySqlDao().GetTopProducts(startDate, endDate);
            var topCategoriesData = await new MySqlDao().GetTopCategories(startDate, endDate);
            var topSellersData = await new MySqlDao().GetTopSellers(startDate, endDate);

            // Cập nhật các thuộc tính với dữ liệu từ khoảng thời gian
            OrderCount = revenueData.OrderCount;
            TotalRevenue = revenueData.TotalRevenue;
            CashAmount = revenueData.CashAmount;
            TopProducts = topProductsData;
            TopCategories = topCategoriesData;
            TopSellers = topSellersData;

            // Thông báo thay đổi dữ liệu
            OnPropertyChanged(nameof(OrderCount));
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(CashAmount));
            OnPropertyChanged(nameof(TransferAmount));
            OnPropertyChanged(nameof(TotalRevenueFormatted));
            OnPropertyChanged(nameof(CashAmountFormatted));
            OnPropertyChanged(nameof(TransferAmountFormatted));
            OnPropertyChanged(nameof(TopProducts));
            OnPropertyChanged(nameof(TopCategories));
            OnPropertyChanged(nameof(TopSellers));
        }
    }
}
