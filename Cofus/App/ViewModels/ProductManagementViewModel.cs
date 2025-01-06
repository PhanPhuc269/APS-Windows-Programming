using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using App.Model;
using System.Diagnostics;

namespace App.ViewModels
{
    public class ProductManagementViewModel : ObservableObject
    {
        private FullObservableCollection<Category> _listTypeBeverages;
        public FullObservableCollection<Category> ListTypeBeverages
        {
            get => _listTypeBeverages;
            set => SetProperty(ref _listTypeBeverages, value);
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        public bool CanDelete => SelectedCategory != null;

        public Category CurrentCategoryForBeverage
        {
            get; set;
        }

        public ProductManagementViewModel()
        {
            IDao dao = App.GetService<IDao>();
            // Lấy danh sách danh mục từ DAO và lọc bỏ danh mục "Tất cả"
            var allCategories = dao.GetListTypeBeverage();
            ListTypeBeverages = new FullObservableCollection<Category>(
                allCategories.Where(category => !category.Name.Equals("All", StringComparison.OrdinalIgnoreCase))
            );

            foreach (var category in ListTypeBeverages)
            {
                Debug.WriteLine($"Category: {category.Name}, Status: {category.Status}");
            }
        }

        public bool AddCategory(string categoryName, string imagePath)
        {
            try
            {
                IDao dao = App.GetService<IDao>();
                bool isAdded = dao.AddCategory(categoryName, imagePath);

                if (isAdded)
                {
                    ListTypeBeverages.Add(new Category(categoryName, new FullObservableCollection<Product>()));
                }

                return isAdded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi thêm danh mục: {ex.Message}");
                return false;
            }
        }

        // Phương thức mới để thêm Beverage với công thức
        public bool AddBeverage(Category category, string beverageName, string imagePath,
                                List<(string size, decimal price)> sizesAndPrices,
                                List<(string materialCode, int quantity)> recipe)
        {
            try
            {
                IDao dao = App.GetService<IDao>();
                bool isAdded = dao.AddBeverage(category.Name, beverageName, imagePath, sizesAndPrices, recipe);

                if (isAdded)
                {
                    var newProduct = new Product
                    {
                        Name = beverageName,
                        Image = imagePath
                    };
                    category.Products.Add(newProduct);
                }

                return isAdded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding Beverage: {ex.Message}");
                return false;
            }
        }

        public bool DeleteCategory(Category category)
        {
            try
            {
                IDao dao = App.GetService<IDao>();
                bool isDeleted = dao.DeleteCategory(category.Name);

                if (isDeleted)
                {
                    ListTypeBeverages.Remove(category);
                }

                return isDeleted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi xóa danh mục: {ex.Message}");
                return false;
            }
        }

        public bool UpdateBeverage(Category category, Product updatedBeverage, List<(string size, decimal price)> sizesAndPrices, int status)
        {
            try
            {
                IDao dao = App.GetService<IDao>();

                // Gọi phương thức DAO để cập nhật thông tin Beverage
                bool isUpdated = dao.UpdateBeverage(category, updatedBeverage, sizesAndPrices, status);

                if (isUpdated)
                {
                    // Tìm và cập nhật thông tin trong danh sách sản phẩm của danh mục
                    var beverage = category.Products.FirstOrDefault(p => p.Id == updatedBeverage.Id);
                    if (beverage != null)
                    {
                        beverage.Name = updatedBeverage.Name;
                        beverage.Image = updatedBeverage.Image;
                    }
                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating Beverage: {ex.Message}");
                return false;
            }
        }


        public List<(string size, decimal price)> GetSizesAndPrices(int beverageId)
        {
            IDao dao = App.GetService<IDao>();
            return dao.GetSizesAndPrices(beverageId);
        }

        public List<(string size, string materialName, int quantity, string unit)> GetRecipe(int beverageId)
        {
            IDao dao = App.GetService<IDao>();
            return dao.GetRecipe(beverageId);
        }

        public bool ToggleCategoryStatus(Category category)
        {
            try
            {
                IDao dao = App.GetService<IDao>();

                // Đổi trạng thái: Nếu trạng thái hiện tại là 1 thì đổi thành 0, ngược lại đổi thành 1
                int newStatus = category.Status == 1 ? 0 : 1;

                // Gọi DAO để cập nhật trạng thái
                bool isUpdated = dao.UpdateCategoryStatus(category.Name, newStatus);

                if (isUpdated)
                {
                    // Cập nhật trạng thái trong danh sách
                    category.Status = newStatus;
                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling category status: {ex.Message}");
                return false;
            }
        }


        public bool ToggleBeverageStatus(Product beverage)
        {
            try
            {
                IDao dao = App.GetService<IDao>();

                // Đổi trạng thái
                int newStatus = beverage.Status == 1 ? 0 : 1;

                // Cập nhật trạng thái trong cơ sở dữ liệu
                bool isUpdated = dao.UpdateBeverageStatus(beverage.Id, newStatus);

                if (isUpdated)
                {
                    // Cập nhật trạng thái trong danh sách
                    beverage.Status = newStatus;
                }

                return isUpdated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling beverage status: {ex.Message}");
                return false;
            }
        }

    }
}
