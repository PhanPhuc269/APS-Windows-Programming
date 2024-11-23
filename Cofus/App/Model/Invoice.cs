using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PropertyChanged;

namespace App.Model;

[AddINotifyPropertyChangedInterface]
public class Invoice : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int InvoiceNumber { get; set; }
    public DateTime CreatedTime { get; set; }

    public DateTime CompleteTime
    {
        get; set;
    }
    public int TableNumber { get; set; }
    public string PaymentMethod
    {
        get; set;
    }

    public FullObservableCollection<InvoiceItem> InvoiceItems { get; set; }

    public Invoice()
    {
        // Khởi tạo InvoiceItems
        InvoiceItems = new FullObservableCollection<InvoiceItem>();

        // Đăng ký sự kiện CollectionChanged để cập nhật tổng số lượng và tổng giá khi danh sách thay đổi
        InvoiceItems.CollectionChanged += OnInvoiceItemsChanged;
    }

    private void OnInvoiceItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Khi có các mục mới được thêm vào, đăng ký sự kiện PropertyChanged cho chúng
        if (e.NewItems != null)
        {
            foreach (InvoiceItem item in e.NewItems)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        // Khi có các mục cũ bị xóa, hủy đăng ký sự kiện PropertyChanged
        if (e.OldItems != null)
        {
            foreach (InvoiceItem item in e.OldItems)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }

        // Cập nhật tổng số lượng và tổng tiền
        UpdateTotals();
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Kiểm tra nếu thuộc tính thay đổi là "Quantity" hoặc "Total"
        if (e.PropertyName == nameof(InvoiceItem.Quantity) || e.PropertyName == nameof(InvoiceItem.Total))
        {
            // Cập nhật tổng số lượng và tổng tiền khi thuộc tính Quantity hoặc Total thay đổi
            UpdateTotals();
        }
    }

    public void UpdateTotals()
    {
        OnPropertyChanged(nameof(TotalQuantity));
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(AmountDue));
    }

    // Phương thức để tính tổng số lượng
    public int TotalQuantity => InvoiceItems.Sum(item => item.Quantity);

    // Phương thức để tính tổng giá
    public int TotalPrice => InvoiceItems.Sum(item => item.Total);

    public int ConsumedPoints
    {
        get; set;
    }=0;
    public int AmountDue => TotalPrice - ConsumedPoints;
    public string? CustomerPhoneNumber
    {
        get; set;
    }

    // Phương thức để thêm sản phẩm vào hóa đơn
    public void AddItem(InvoiceItem item)
    {
        // Đăng ký sự kiện PropertyChanged để cập nhật khi sản phẩm được thay đổi
        item.PropertyChanged += Item_PropertyChanged;
        InvoiceItems.Add(item);
    }

    // Phương thức để xóa sản phẩm khỏi hóa đơn
    public void RemoveItem(InvoiceItem item)
    {
        // Hủy đăng ký sự kiện PropertyChanged trước khi xóa
        item.PropertyChanged -= Item_PropertyChanged;
        InvoiceItems.Remove(item);
    }

    // Phương thức để đánh dấu hóa đơn đã thanh toán
    public void MarkAsPaid()
    {
        // Implementation for marking the invoice as paid
    }

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
