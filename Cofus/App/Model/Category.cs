using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;

public class Category :INotifyPropertyChanged
{
    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public Category(string name, FullObservableCollection<Product> products)
    {
        Name = name;
        Products = products;
    }
    public FullObservableCollection<Product> Products
    {
        get; set;
    }
    public event PropertyChangedEventHandler PropertyChanged;
}
