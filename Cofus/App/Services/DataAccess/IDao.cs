using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Model;
using App.Views;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;

namespace App;
public interface IDao
{
    Category GetCategory(string type);
    Category GetCategoryHome(string type);

    FullObservableCollection<Category> GetListTypeBeverage();
    FullObservableCollection<Category> GetListTypeBeverageHome();
    FullObservableCollection<Invoice> GetPendingOrders();

    Task<List<string>> SuggestCustomerPhoneNumbers(string keyword);
    Task<int> CreateOrder(Invoice invoice);
    Task AddOrderDetail(int orderId, InvoiceItem item);
    int GetBeverageSizeId(int beverageId, string size);
    Task<int> GetMaxAvailableQuantityAsync(int beverageSizeId);
    List<string> GetAllPaymentMethod();
    bool CompletePendingOrder(Invoice order);
    FullObservableCollection<Product> GetAllBeverage();
    FullObservableCollection<Product> GetAllBeverageHome();

    int GetProductPrice(int beverageId, string size);
    int GetComsumedPoints(string phoneNumber);
    void BonusPoints(int AmountDue, string CustomerPhone);
    void ConsumePoints(int points, string customerPhone);
    (FullObservableCollection<Customer> Customers, int TotalCount) GetCustomers(int pageNumber, int pageSize, string? name = null, string? phoneNumber = null, int? minPoints = null, int? maxPoints = null);
    bool AddCustomer(Customer customer);
    bool UpdateCustomer(Customer customer);
    bool DeleteCustomer(int customerId);
    // Revenue
    Task<Revenue> GetRevenue(DateTime startDate, DateTime endDate);
    Task<List<TopProduct>> GetTopProducts(DateTime startDate, DateTime endDate);
    Task<List<TopCategory>> GetTopCategories(DateTime startDate, DateTime endDate);
    Task<List<TopSeller>> GetTopSellers(DateTime startDate, DateTime endDate);

    // Quản lý nguyên liệu
    List<Material> GetAllMaterials();

    List<Material> getAllThreshold();
    Material GetMaterialByCode(string code);
    bool AddMaterial(Material material);
    bool UpdateMaterial(Material material);
    bool DeleteMaterial(string code);

    // Thông báo ngưỡng hết nguyn lịu
    bool UpdateMaterialThreshold(string materialCode, int newThreshold);
    List<Material> GetAllMaterialsOutStock();



    // Quản lý người dùng
    List<User> GetAllUsers();
    User GetUserByUsername(string username);
    bool AddUser(User user);

    User GetUserByEmail(string email);
    bool UpdateUser(User user);
    bool AddCategory(string categoryName, string imagePath);
    bool DeleteCategory(string categoryName);
    bool AddBeverage(string categoryName, string beverageName, string imagePath,
                            List<(string size, decimal price)> sizesAndPrices,
                            List<(string materialCode, int quantity)> recipe);

    bool UpdateBeverage(Category category, Product updatedBeverage, List<(string size, decimal price)> sizesAndPrices, int status);
    List<(string size, decimal price)> GetSizesAndPrices(int beverageId);

    List<(string size, string materialName, int quantity, string unit)> GetRecipe(int beverageId);

    bool UpdateCategoryStatus(string categoryName, int newStatus);

    List<HistoryOrder> GetBeveragesPurchasedByCustomer(string phoneNumber);

    //Quản lý ca làm việc
    List<ShiftAttendance> GetShiftAttendances(DateTime startDate, DateTime endDate);
    Task<bool> AddShiftAttendance(Shift shift, int employeeId);
    Task<bool> CheckShiftAttendance(int employeeId, Shift shift);

    List<User> SearchEmployees(string keyword);
    public bool DeleteEmployee(string userId);

    bool UpdateEmployee(User user);
    bool UpdateBeverageStatus(int beverageId, int newStatus);

}
