using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace App.Model;

[AddINotifyPropertyChangedInterface]
public class Customer: INotifyPropertyChanged
{
    public int CustomerId
    {
        get; set;
    }
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber
    {
        get; set;
    }
    public string? Email
    {
        get; set;
    }
    public int Points { get; set; } = 0;
    public event PropertyChangedEventHandler PropertyChanged;
}
