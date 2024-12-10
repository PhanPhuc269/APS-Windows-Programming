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

            ViewModel.SelectedDate = DateTime.Today;
            DatePicker.SelectedDate = ViewModel.SelectedDate;  // Cập nhật lại cho DatePicker


            // Gọi LoadRevenueData mặc định với ngày hiện tại
            LoadData(ViewModel.SelectedDate);
        }

        // Đảm bảo khi ngày thay đổi sẽ gọi lại LoadRevenueData
        private async void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            // Kiểm tra nếu NewDate có giá trị
            if (e.NewDate != null)
            {
                // Cập nhật ngày được chọn trong ViewModel
                ViewModel.SelectedDate = e.NewDate.Date;

                // Gọi phương thức LoadRevenueData với ngày mới
                await ViewModel.LoadRevenueData(ViewModel.SelectedDate);

                // Cập nhật lại giá trị cho DatePicker (đảm bảo DatePicker hiển thị đúng)
                DatePicker.SelectedDate = ViewModel.SelectedDate;
            }
        }


        // Thay đổi phương thức LoadData để sử dụng ngày được chọn
        private async void LoadData(DateTime selectedDate)
        {
            await ViewModel.LoadRevenueData(selectedDate);
        }
    }
}
