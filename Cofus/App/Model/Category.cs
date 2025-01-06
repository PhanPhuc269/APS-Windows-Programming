using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model
{
    public class Category : INotifyPropertyChanged
    {
        private int _status; // 1: Hoạt động, 0: Không hoạt động

        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public int Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public FullObservableCollection<Product> Products
        {
            get; set;
        }

        public Category(string name, FullObservableCollection<Product> products, int status = 1)
        {
            Name = name;
            Products = products;
            Status = status;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
