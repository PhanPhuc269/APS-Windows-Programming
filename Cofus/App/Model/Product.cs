using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace App.Model
{
    public class Product : INotifyPropertyChanged
    {
        private int _status;

        public int Id
        {
            get; set;
        }
        public int TypeBeverageId
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public decimal Price
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

        // Thuộc tính trạng thái (Bật/Tắt)
        public int Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsOn)); // Gửi thông báo thay đổi cho IsOn
                }
            }
        }

        // Thuộc tính IsOn để hỗ trợ binding với ToggleSwitch
        public bool IsOn
        {
            get => _status == 1;
            set
            {
                Status = value ? 1 : 0;
                OnPropertyChanged(nameof(IsOn));
            }
        }

        public List<(string size, decimal price)> SizesAndPrices { get; set; } = new List<(string size, decimal price)>();

        // Danh sách công thức
        public List<RecipeItem> Recipe { get; set; } = new List<RecipeItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
