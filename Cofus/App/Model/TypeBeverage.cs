using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;
public class TypeBeverage: INotifyPropertyChanged
{
    public int Id
    {
        get; set;
    }
    public string TypeName
    {
        get; set;
    }
    public FullObservableCollection<Product> Products { get; set; }
    public TypeBeverage(string name, FullObservableCollection<Product> products)
    {
        TypeName = name;
        Products = products;
    }
    public event PropertyChangedEventHandler PropertyChanged;
}
