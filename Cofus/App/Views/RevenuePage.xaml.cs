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

            ViewModel.StartDate = DateTime.Today; // Ngày bắt đầu mặc định là hôm nay
            ViewModel.EndDate = DateTime.Today;   // Ngày kết thúc mặc định là hôm nay

            StartDatePicker.SelectedDate = ViewModel.StartDate;
            EndDatePicker.SelectedDate = ViewModel.EndDate;

            // Gọi LoadRevenueData mặc định với khoảng thời gian hiện tại
            LoadData(ViewModel.StartDate, ViewModel.EndDate);
        }

        // Khi ngày bắt đầu thay đổi
        private async void StartDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                ViewModel.StartDate = e.NewDate.Date;

                // Đảm bảo ngày bắt đầu không lớn hơn ngày kết thúc
                if (ViewModel.StartDate > ViewModel.EndDate)
                {
                    ViewModel.EndDate = ViewModel.StartDate;
                    EndDatePicker.SelectedDate = ViewModel.EndDate;
                }

                await LoadData(ViewModel.StartDate, ViewModel.EndDate);
            }
        }

        // Khi ngày kết thúc thay đổi
        private async void EndDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate != null)
            {
                ViewModel.EndDate = e.NewDate.Date;

                // Đảm bảo ngày kết thúc không nhỏ hơn ngày bắt đầu
                if (ViewModel.EndDate < ViewModel.StartDate)
                {
                    ViewModel.StartDate = ViewModel.EndDate;
                    StartDatePicker.SelectedDate = ViewModel.StartDate;
                }

                await LoadData(ViewModel.StartDate, ViewModel.EndDate);
            }
        }

        // Phương thức LoadData để tải dữ liệu theo khoảng thời gian
        private async System.Threading.Tasks.Task LoadData(DateTime startDate, DateTime endDate)
        {
            await ViewModel.LoadRevenueData(startDate, endDate);
        }
    }
}
