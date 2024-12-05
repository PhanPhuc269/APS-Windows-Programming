using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Model;
using App.Views;
using Microsoft.UI.Xaml.Controls;

namespace App;
public interface IDao
{
    Category GetCategory(string type);
    FullObservableCollection<Category> GetListTypeBeverage();
    FullObservableCollection<Invoice> GetPendingOrders();
    Task<List<string>> SuggestCustomerPhoneNumbers(string keyword);
    Task<int> CreateOrder(Invoice invoice);
    Task AddOrderDetail(int orderId, InvoiceItem item);
    List<string> GetAllPaymentMethod();
    bool CompletePendingOrder(Invoice order);
    FullObservableCollection<Product> GetAllBeverage();
    int GetProductPrice(int beverageId, string size);
    int GetComsumedPoints(string phoneNumber);
    void BonusPoints(int AmountDue, string CustomerPhone);
    void ConsumePoints(int points, string customerPhone);
    (FullObservableCollection<Customer> Customers, int TotalCount) GetCustomers(int pageNumber, int pageSize, string? name = null, string? phoneNumber = null, int? minPoints = null, int? maxPoints = null);
    bool AddCustomer(Customer customer);
    bool UpdateCustomer(Customer customer);
    bool DeleteCustomer(int customerId);
    // Revenue
    Task<Revenue> GetRevenue(DateTime selectedDate, DateTime previousDate);
    Task<List<TopProduct>> GetTopProducts(DateTime selectedDate);
    Task<List<TopCategory>> GetTopCategories(DateTime selectedDate);
    Task<List<TopSeller>> GetTopSellers(DateTime selectedDate);

    // Quản lý nguyên liệu
    List<Material> GetAllMaterials();
    Material GetMaterialByCode(string code);
    bool AddMaterial(Material material);
    bool UpdateMaterial(Material material);
    bool DeleteMaterial(string code);



    // Quản lý người dùng
    List<User> GetAllUsers();
    User GetUserByUsername(string username);
    bool AddUser(User user);
}
