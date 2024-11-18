using System;
using Microsoft.UI.Xaml;
using PropertyChanged;
using System.ComponentModel;
using App.Model;

namespace App.ViewModels;

[AddINotifyPropertyChangedInterface]
public class InvoiceControlViewModel : INotifyPropertyChanged
{
    private DispatcherTimer _timer;
    private DateTime _currentDateTime;
    private bool _isPaid;

    public event PropertyChangedEventHandler PropertyChanged;

    public DateTime CurrentDateTime
    {
        get => _currentDateTime;
        set
        {
            _currentDateTime = value;
            OnPropertyChanged(nameof(CurrentDate));
            OnPropertyChanged(nameof(CurrentTime));
        }
    }

    public Invoice Invoice { get; set; }

    public int TotalQuantity => Invoice.TotalQuantity;
    public int TotalPrice => Invoice.TotalPrice;
    public bool IsPaid
    {
        get => _isPaid;
        set
        {
            _isPaid = value;
            OnPropertyChanged(nameof(IsPaid));
        }
    }
    public InvoiceControlViewModel()
    {
        Invoice = new Invoice();
        _currentDateTime = DateTime.Now;
        _isPaid = false;

        // Thiết lập timer để cập nhật thời gian mỗi giây
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
        // Subscribe to PropertyChanged event of Invoice to update totals
        Invoice.PropertyChanged += Invoice_PropertyChanged;
    }

    private void Timer_Tick(object sender, object e)
    {
        // Cập nhật thời gian hiện tại mỗi giây
        CurrentDateTime = DateTime.Now;
    }

    // Định dạng ngày giờ cho giao diện
    public string CurrentDate => CurrentDateTime.ToString("dd/MM/yyyy");
    public string CurrentTime => CurrentDateTime.ToString("HH:mm:ss");

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddInvoiceItem(InvoiceItem invoiceItem)
    {
        Invoice.AddItem(invoiceItem);
        OnPropertyChanged(nameof(TotalQuantity));
        OnPropertyChanged(nameof(TotalPrice));
    }

    public FullObservableCollection<InvoiceItem> GetInvoiceItems()
    {
        return Invoice.InvoiceItems;
    }

    private void Invoice_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Invoice.TotalQuantity) || e.PropertyName == nameof(Invoice.TotalPrice))
        {
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
    public async Task<bool> Checkout()
    {
        var dao = App.GetService<IDao>();

        try
        {
            int orderId = await dao.CreateOrder(Invoice);

            // Tạo một bản sao của collection để tránh lỗi
            var invoiceItemsCopy = Invoice.InvoiceItems.ToList();

            foreach (var item in invoiceItemsCopy)
            {
                await dao.AddOrderDetail(orderId, item);
            }

            // Dừng tăng thời gian
            _timer.Stop();
            CurrentDateTime = Invoice.CreatedTime;
            IsPaid = true;
            return true;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log error)
            Console.WriteLine($"Checkout failed: {ex.Message}");
            return false;
        }
    }
}
