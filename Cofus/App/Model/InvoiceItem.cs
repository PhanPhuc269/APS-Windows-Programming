using System.ComponentModel;
using PropertyChanged;

namespace App.Model;

[AddINotifyPropertyChangedInterface]
public class InvoiceItem : INotifyPropertyChanged
{
    private int _quantity;
    public int BeverageId
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }
    public int Price
    {
        get; set;
    }
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
    }
    public string Size
    {
        get; set;
    }
    public string Note
    {
        get; set;
    }

    public int Total => Price * Quantity;

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
