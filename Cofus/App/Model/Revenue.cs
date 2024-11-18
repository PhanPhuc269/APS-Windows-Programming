using System.ComponentModel;

namespace App.Model;
public class Revenue : INotifyPropertyChanged
{
    private int _orderCount;
    private int _totalRevenue;
    private int _cashAmount;

    public int OrderCount
    {
        get => _orderCount;
        set
        {
            if (_orderCount != value)
            {
                _orderCount = value;
                OnPropertyChanged(nameof(OrderCount));
            }
        }
    }

    public int TotalRevenue
    {
        get => _totalRevenue;
        set
        {
            if (_totalRevenue != value)
            {
                _totalRevenue = value;
                OnPropertyChanged(nameof(TotalRevenue));
            }
        }
        
    }

    public int CashAmount
    {
        get => _cashAmount;
        set
        {
            if (_cashAmount != value)
            {
                _cashAmount = value;
                OnPropertyChanged(nameof(CashAmount));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TopProduct
{
    public string ImageUrl
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public int Revenue
    {
        get; set;
    }
    public string RevenueFormatted => $"{Revenue:N0} VND";
}

public class TopCategory
{
    public string ImageUrl
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public int Revenue
    {
        get; set;
    }
    public string RevenueFormatted => $"{Revenue:N0} VND";
}

public class TopSeller
{
    public string ImageUrl
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public int Amount
    {
        get; set;
    }
}
