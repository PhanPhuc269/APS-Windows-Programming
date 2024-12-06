using System;
using System.ComponentModel;
using System.Linq;
using App.Model;
using Microsoft.UI.Xaml;

namespace App.ViewModels
{
    public class PendingControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FullObservableCollection<Invoice> _pendingInvoices;
        public FullObservableCollection<Invoice> PendingInvoices
        {
            get
            {
                return _pendingInvoices;
            }
            set
            {
                if (_pendingInvoices != value)
                {
                    _pendingInvoices = value;
                    OnPropertyChanged(nameof(PendingInvoices));
                }
            }
        }

        private DispatcherTimer _timer;

        public PendingControlViewModel()
        {
            // Lấy dữ liệu từ DAO
            IDao dao = App.GetService<IDao>();
            PendingInvoices = dao.GetPendingOrders();

            // Khởi tạo Timer để cập nhật mỗi giây
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);  // Mỗi giây
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            // Cập nhật thời gian còn lại của mỗi hóa đơn
            foreach (var invoice in PendingInvoices)
            {
                // Truy cập `RemainingTimeFormatted` sẽ tự động tính toán và hiển thị thời gian còn lại theo định dạng hh:mm:ss
                var remainingTime = invoice.RemainingTime;

                // Nếu thời gian còn lại bằng 00:00:00, bạn có thể làm gì đó (ví dụ: đánh dấu hóa đơn đã hết hạn)
                if (remainingTime == "00:00:00")
                {
                    // Thực hiện hành động khi hóa đơn hết thời gian
                    // Ví dụ: Đánh dấu hóa đơn đã hết thời gian hoặc thực hiện các thay đổi UI
                }
            }

            // Sau khi tính toán lại thời gian còn lại, thông báo UI cập nhật
            OnPropertyChanged(nameof(PendingInvoices));
        }


        // Phương thức để hoàn thành đơn hàng
        public void CompleteOrder(Invoice order)
        {
            PendingInvoices.Remove(order);
            order.CompleteTime = DateTime.Now;
            App.GetService<IDao>().CompletePendingOrder(order);
        }

        // Hàm thông báo khi có sự thay đổi thuộc tính
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
