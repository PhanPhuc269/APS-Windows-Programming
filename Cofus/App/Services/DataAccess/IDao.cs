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
    Task<int> CreateOrder(Invoice invoice);
    Task AddOrderDetail(int orderId, InvoiceItem item);
    List<string> GetAllPaymentMethod();
    bool CompletePendingOrder(Invoice order);
    FullObservableCollection<Product> GetAllBeverage();
    int GetProductPrice(int beverageId, string size);

    // Revenue
    Task<Revenue> GetRevenue(DateTime selectedDate, DateTime previousDate);
    Task<List<TopProduct>> GetTopProducts(DateTime selectedDate);
    Task<List<TopCategory>> GetTopCategories(DateTime selectedDate);
    Task<List<TopSeller>> GetTopSellers(DateTime selectedDate);


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
