using App.Model;
using App.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace App.Views
{
    public sealed partial class ProductManagementPage : Page
    {
        public ProductManagementViewModel ViewModel
        {
            get;
        }

        private Product _selectedBeverage;

        // Khai báo ObservableCollection để lưu trữ các nguyên liệu trong công thức
        public ObservableCollection<RecipeItem> RecipeItems { get; set; } = new ObservableCollection<RecipeItem>();

        public ProductManagementPage()
        {
            ViewModel = App.GetService<ProductManagementViewModel>();
            InitializeComponent();
            RecipeListView.ItemsSource = RecipeItems;
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Product clickedProduct)
            {
                Debug.WriteLine($"Product clicked: {clickedProduct.Name}");
            }
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị dialog thêm danh mục
            await AddCategoryDialog.ShowAsync();
        }

        private void AddCategoryDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Lấy thông tin từ dialog
            string categoryName = CategoryNameTextBox.Text.Trim();
            string imagePath = ImagePathTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(imagePath))
            {
                Debug.WriteLine("Tên danh mục hoặc đường dẫn ảnh không hợp lệ.");
                return;
            }

            // Gọi ViewModel để thêm danh mục
            bool success = ViewModel.AddCategory(categoryName, imagePath);
            if (success)
            {
                Debug.WriteLine("Thêm danh mục thành công.");
            }
            else
            {
                Debug.WriteLine("Thêm danh mục thất bại.");
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCategory == null)
            {
                Debug.WriteLine("Không có danh mục nào được chọn để xóa.");
                return;
            }

            // Hiển thị dialog xác nhận xóa
            await DeleteCategoryDialog.ShowAsync();
        }

        private void DeleteCategoryDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel.SelectedCategory != null)
            {
                // Gọi ViewModel để xóa danh mục
                bool success = ViewModel.DeleteCategory(ViewModel.SelectedCategory);
                if (success)
                {
                    Debug.WriteLine("Xóa danh mục thành công.");
                }
                else
                {
                    Debug.WriteLine("Xóa danh mục thất bại.");
                }
            }
        }

        private async void AddBeverage_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCategory != null)
            {
                ViewModel.CurrentCategoryForBeverage = ViewModel.SelectedCategory;
                await AddBeverageDialog.ShowAsync();
            }
            else
            {
                Debug.WriteLine("Không có danh mục nào được chọn.");
            }
        }

        // Xử lý sự kiện khi nhấn nút "Thêm" trong AddBeverageDialog
        private void AddBeverageDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string beverageName = BeverageNameTextBox.Text.Trim();
            string imagePath = BeverageImagePathTextBox.Text.Trim();
            string size = BeverageSizeTextBox.Text.Trim();
            string priceInput = BeveragePriceTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(beverageName) || string.IsNullOrWhiteSpace(imagePath) ||
                string.IsNullOrWhiteSpace(size) || string.IsNullOrWhiteSpace(priceInput) ||
                !decimal.TryParse(priceInput, out decimal price))
            {
                Debug.WriteLine("Thông tin không hợp lệ.");
                return;
            }

            // Validate recipe items
            var recipe = new List<(string materialCode, int quantity)>();
            foreach (var item in RecipeItems)
            {
                if (string.IsNullOrWhiteSpace(item.MaterialCode) || item.Quantity <= 0)
                {
                    Debug.WriteLine("Thông tin công thức không hợp lệ.");
                    return;
                }
                recipe.Add((item.MaterialCode.Trim(), item.Quantity));
            }

            // Gọi ViewModel để thêm Beverage với công thức
            bool success = ViewModel.AddBeverage(ViewModel.CurrentCategoryForBeverage, beverageName, imagePath,
                new List<(string size, decimal price)>
                {
                    (size, price)
                }, recipe);

            if (success)
            {
                Debug.WriteLine("Thêm Beverage thành công.");
                // Làm sạch danh sách công thức sau khi thêm thành công
                RecipeItems.Clear();
            }
            else
            {
                Debug.WriteLine("Thêm Beverage thất bại.");
            }
        }

        // Xử lý thêm một nguyên liệu mới vào công thức
        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            RecipeItems.Add(new RecipeItem());
        }

        // Xử lý xóa một nguyên liệu khỏi công thức
        private void RemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RecipeItem item)
            {
                RecipeItems.Remove(item);
            }
        }

        // Xử lý sự kiện khi nhấn nút "Sửa" Beverage
        private async void EditBeverage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product beverage)
            {
                _selectedBeverage = beverage;

                // Điền các thông tin khác
                EditBeverageNameTextBox.Text = beverage.Name;
                EditBeverageImagePathTextBox.Text = beverage.Image;
                EditBeverageSizeTextBox.Text = beverage.Size;
                EditBeveragePriceTextBox.Text = beverage.Price.ToString();

                await EditBeverageDialog.ShowAsync();
            }
        }

        private void EditBeverageDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_selectedBeverage != null)
            {
                // Lấy thông tin từ các TextBox
                _selectedBeverage.Name = EditBeverageNameTextBox.Text.Trim();
                _selectedBeverage.Image = EditBeverageImagePathTextBox.Text.Trim();

                // Khởi tạo danh sách sizesAndPrices
                var sizesAndPrices = new List<(string size, decimal price)>();

                // Lấy kích thước và giá từ TextBox
                if (!string.IsNullOrWhiteSpace(EditBeverageSizeTextBox.Text) &&
                    decimal.TryParse(EditBeveragePriceTextBox.Text.Trim(), out decimal price))
                {
                    sizesAndPrices.Add((EditBeverageSizeTextBox.Text.Trim(), price));
                }
                else
                {
                    Debug.WriteLine("Kích thước hoặc giá không hợp lệ.");
                    return; // Dừng nếu thông tin không hợp lệ
                }

                // Gọi phương thức UpdateBeverage
                bool success = ViewModel.UpdateBeverage(ViewModel.SelectedCategory, _selectedBeverage, sizesAndPrices, _selectedBeverage.Status);
                if (success)
                {
                    Debug.WriteLine("Cập nhật Beverage thành công.");
                }
                else
                {
                    Debug.WriteLine("Cập nhật Beverage thất bại.");
                }

                _selectedBeverage = null; // Reset selected beverage
            }
        }



        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                // Hiển thị thông tin tên món nước
                DetailBeverageNameTextBlock.Text = product.Name;

                // Lấy danh sách kích cỡ và giá từ ViewModel
                var sizesAndPrices = ViewModel.GetSizesAndPrices(product.Id);
                DetailSizePriceListView.ItemsSource = sizesAndPrices.Select(sp => $"Kích cỡ: {sp.size}, Giá: {sp.price} VND");

                // Lấy công thức từ ViewModel
                var recipe = ViewModel.GetRecipe(product.Id);

                // Tạo từ điển ánh xạ thứ tự kích cỡ
                var sizeOrder = new Dictionary<string, int> { { "S", 1 }, { "M", 2 }, { "L", 3 } };

                // Nhóm công thức theo kích cỡ và sắp xếp trực tiếp bằng OrderBy
                DetailRecipeListView.ItemsSource = recipe
                    .GroupBy(r => r.size)
                    .OrderBy(g => sizeOrder.ContainsKey(g.Key) ? sizeOrder[g.Key] : int.MaxValue) // Sắp xếp S, M, L, các kích cỡ khác sẽ ở cuối
                    .SelectMany(g => new[] { $"Kích cỡ: {g.Key}" }.Concat(
                        g.Select(r => $"{r.materialName} - {r.quantity} {r.unit}")
                    ));

                // Hiển thị dialog
                await ViewDetailsDialog.ShowAsync();
            }
        }

        private void ToggleCategoryStatus_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCategory == null)
            {
                Debug.WriteLine("Không có danh mục nào được chọn để đổi trạng thái.");
                return;
            }

            bool success = ViewModel.ToggleCategoryStatus(ViewModel.SelectedCategory);
            if (success)
            {
                Debug.WriteLine($"Đổi trạng thái danh mục '{ViewModel.SelectedCategory.Name}' thành công.");
            }
            else
            {
                Debug.WriteLine($"Đổi trạng thái danh mục '{ViewModel.SelectedCategory.Name}' thất bại.");
            }
        }


        private void ToggleBeverageStatus_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch && toggleSwitch.DataContext is Product beverage)
            {
                bool success = ViewModel.ToggleBeverageStatus(beverage);
                if (!success)
                {
                    Debug.WriteLine($"Failed to toggle status for beverage: {beverage.Name}");
                    // Nếu thất bại, hoàn nguyên trạng thái
                    toggleSwitch.IsOn = !toggleSwitch.IsOn;
                }
            }
        }


    }
}