using CommunityToolkit.Mvvm.ComponentModel;

namespace App.Model
{
    public class RecipeItem : ObservableObject
    {
        private string _materialCode;
        public string MaterialCode
        {
            get => _materialCode;
            set => SetProperty(ref _materialCode, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
    }
}
