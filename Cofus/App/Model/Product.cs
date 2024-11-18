using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;
public class Product :INotifyPropertyChanged
{
    public int Id { get; set; }
    public int TypeBeverageId
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
    public string Image
    {
        get; set;
    }
    public string Size
    {
        get; set;
    }
    public event PropertyChangedEventHandler PropertyChanged;
}

