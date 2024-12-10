using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using App.Model;
using Microsoft.UI.Xaml;

namespace App.ViewModels;

public class PendingControlViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private DispatcherTimer _timer;

    private FullObservableCollection<Invoice> _pendingInvoices;
    public FullObservableCollection<Invoice> PendingInvoices
    {
        get => _pendingInvoices;
        set
        {
            if (_pendingInvoices != value)
            {
                if (_pendingInvoices != null)
                {
                    _pendingInvoices.CollectionChanged -= PendingInvoices_CollectionChanged;
                    foreach (var invoice in _pendingInvoices)
                    {
                        invoice.PropertyChanged -= Invoice_PropertyChanged;
                    }
                }

                _pendingInvoices = value;

                if (_pendingInvoices != null)
                {
                    _pendingInvoices.CollectionChanged += PendingInvoices_CollectionChanged;
                    foreach (var invoice in _pendingInvoices)
                    {
                        invoice.PropertyChanged += Invoice_PropertyChanged;
                    }
                }

                OnPropertyChanged(nameof(PendingInvoices));
            }
        }
    }

    private Dictionary<int, string> _remainingTimes = new();
    public Dictionary<int, string> RemainingTimes
    {
        get => _remainingTimes;
        set
        {
            _remainingTimes = value;
            OnPropertyChanged(nameof(RemainingTimes));
        }
    }

    public PendingControlViewModel()
    {
        IDao dao = App.GetService<IDao>();
        PendingInvoices = dao.GetPendingOrders();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, object e)
    {
        foreach (var invoice in PendingInvoices)
        {
            if (invoice.EstimateTime.HasValue)
            {
                // Update the dictionary with the new RemainingTime
                RemainingTimes[invoice.InvoiceNumber] = invoice.RemainingTime;
            }
         }

        // Notify UI to update RemainingTime bindings
        OnPropertyChanged(nameof(RemainingTimes));
    }

    private void PendingInvoices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Invoice invoice in e.NewItems)
            {
                invoice.PropertyChanged += Invoice_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (Invoice invoice in e.OldItems)
            {
                invoice.PropertyChanged -= Invoice_PropertyChanged;
            }
        }
    }

    private void Invoice_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var invoice = sender as Invoice;

        if (e.PropertyName == nameof(Invoice.TotalPrice))
        {
            Console.WriteLine($"Invoice {invoice?.InvoiceNumber} TotalPrice updated: {invoice?.TotalPrice}");
        }
    }

    public void CompleteOrder(Invoice order)
    {
        PendingInvoices.Remove(order);
        order.CompleteTime = DateTime.Now;
        App.GetService<IDao>().CompletePendingOrder(order);
    }

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
