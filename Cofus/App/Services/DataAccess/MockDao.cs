using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using App.Model;
using App.ViewModels;
using App.Views;
using Microsoft.UI.Xaml.Controls;

namespace App;

public class MockDao : IDao
{
    private readonly FullObservableCollection<Category> _categories;
    private readonly FullObservableCollection<Invoice> _pendingOrders;
    private readonly FullObservableCollection<Product> _products;

    public MockDao()
    {
        // Dữ liệu mẫu cho BEVERAGE với mã loại thức uống
        _products = new()
        {
            new() { Id = 1, Name = "Latte", Image = "/Assets/latte.jpg", TypeBeverageId = 1 },
            new() { Id = 2, Name = "Green Tea", Image = "/Assets/green_tea.jpg", TypeBeverageId = 2 },
            new() { Id = 3, Name = "Milk Tea", Image = "/Assets/milk_tea.jpg", TypeBeverageId = 2 },
            new() { Id = 4, Name = "Orange Juice", Image = "/Assets/orange_juice.jpg", TypeBeverageId = 3 },
            new() { Id = 5, Name = "Espresso", Image = "/Assets/espresso.jpg", TypeBeverageId = 1 },
            new() { Id = 6, Name = "Black Coffee", Image = "/Assets/black_coffee.jpg", TypeBeverageId = 1 },
            new() { Id = 7, Name = "Oolong Tea", Image = "/Assets/oolong_tea.jpg", TypeBeverageId = 2 },
            new() { Id = 8, Name = "Apple Juice", Image = "/Assets/apple_juice.jpg", TypeBeverageId = 3 },
            new() { Id = 9, Name = "Smoothie", Image = "/Assets/smoothie.jpg", TypeBeverageId = 4 },
            new() { Id = 10, Name = "Iced Latte", Image = "/Assets/iced_latte.jpg", TypeBeverageId = 1 },
            new() { Id = 11, Name = "Matcha Latte", Image = "/Assets/matcha_latte.jpg", TypeBeverageId = 2 },
            new() { Id = 12, Name = "Lemon Tea", Image = "/Assets/lemon_tea.jpg", TypeBeverageId = 2 },
            new() { Id = 13, Name = "Pineapple Juice", Image = "/Assets/pineapple_juice.jpg", TypeBeverageId = 3 },
            new() { Id = 14, Name = "Mango Smoothie", Image = "/Assets/mango_smoothie.jpg", TypeBeverageId = 4 },
            new() { Id = 15, Name = "Cappuccino", Image = "/Assets/cappuccino.jpg", TypeBeverageId = 1 }
        };

        // Dữ liệu mẫu cho CATEGORY
        _categories = new()
        {
            new("All", new FullObservableCollection<Product>(_products.ToList())),
            new("Coffee", new FullObservableCollection<Product>(_products.Where(p => p.TypeBeverageId == 1).ToList())),
            new("Tea", new FullObservableCollection<Product>(_products.Where(p => p.TypeBeverageId == 2).ToList())),
            new("Juice", new FullObservableCollection<Product>(_products.Where(p => p.TypeBeverageId == 3).ToList())),
            new("Smoothie", new FullObservableCollection<Product>(_products.Where(p => p.TypeBeverageId == 4).ToList()))
        };

        // Dữ liệu mẫu cho ORDERS
        _pendingOrders = new()
        {
            new()
            {
                InvoiceNumber = 1,
                TableNumber = 101,
                CreatedTime = DateTime.Parse("2024-10-31 12:40:00"),
                PaymentMethod = "Credit Card",
                InvoiceItems = new FullObservableCollection<InvoiceItem>
                {
                    new InvoiceItem { Name = "Latte", Quantity = 2, Price = 45000},
                    new InvoiceItem { Name = "Green Tea", Quantity = 1, Price = 35000 },
                }
            },
            new()
            {
                InvoiceNumber = 2,
                TableNumber = 102,
                CreatedTime = DateTime.Parse("2024-10-31 13:10:00"),
                PaymentMethod = "Cash",
                InvoiceItems = new FullObservableCollection<InvoiceItem>
                {
                    new InvoiceItem { Name = "Milk Tea", Quantity = 3, Price = 30000 },
                    new InvoiceItem { Name = "Orange Juice", Quantity = 2, Price = 27000 },
                }
            },
            new()
            {
                InvoiceNumber = 3,
                TableNumber = 103,
                CreatedTime = DateTime.Parse("2024-10-31 14:00:00"),
                PaymentMethod = "Debit Card",
                InvoiceItems = new FullObservableCollection<InvoiceItem>
                {
                    new InvoiceItem { Name = "Espresso", Quantity = 1, Price = 45000 },
                    new InvoiceItem { Name = "Black Coffee", Quantity = 2, Price = 37000 },
                }
            },
            new()
            {
                InvoiceNumber = 4,
                TableNumber = 104,
                CreatedTime = DateTime.Parse("2024-10-31 14:30:00"),
                PaymentMethod = "Cash",
                InvoiceItems = new FullObservableCollection<InvoiceItem>
                {
                    new InvoiceItem { Name = "Oolong Tea", Quantity = 3, Price = 35000 },
                    new InvoiceItem { Name = "Apple Juice", Quantity = 1, Price = 29000 },
                }
            },
            new()
            {
                InvoiceNumber = 5,
                TableNumber = 105,
                CreatedTime = DateTime.Parse("2024-10-31 15:00:00"),
                PaymentMethod = "Credit Card",
                InvoiceItems = new FullObservableCollection<InvoiceItem>
                {
                    new InvoiceItem { Name = "Smoothie", Quantity = 2, Price = 45000},
                    new InvoiceItem { Name = "Iced Latte", Quantity = 1, Price = 47000 },
                }
            }
        };
        mockMaterials = new List<Material>
        {
            new Material { MaterialCode = "C001", MaterialName = "Cafe Arabica", Quantity = 100, Category = "Cafe", Unit = "Kg", UnitPrice = 150000, ImportDate = new DateTime(2024, 1, 15), ExpirationDate = new DateTime(2024, 3, 1) },
            new Material { MaterialCode = "C002", MaterialName = "Cafe Robusta", Quantity = 80, Category = "Cafe", Unit = "Kg", UnitPrice = 120000, ImportDate = new DateTime(2024, 2, 10), ExpirationDate = new DateTime(2024, 4, 20) },
            new Material { MaterialCode = "C003", MaterialName = "Cafe Mocha", Quantity = 60, Category = "Cafe", Unit = "Kg", UnitPrice = 180000, ImportDate = new DateTime(2024, 1, 25), ExpirationDate = new DateTime(2024, 3, 15) },
            new Material { MaterialCode = "S001", MaterialName = "Sữa tươi", Quantity = 50, Category = "Sữa", Unit = "Lít", UnitPrice = 20000, ImportDate = new DateTime(2024, 1, 20), ExpirationDate = new DateTime(2024, 2, 28) },
            new Material { MaterialCode = "S002", MaterialName = "Sữa đặc", Quantity = 50, Category = "Sữa", Unit = "Lon", UnitPrice = 20000, ImportDate = new DateTime(2024, 1, 22), ExpirationDate = new DateTime(2024, 4, 5) },
            new Material { MaterialCode = "S003", MaterialName = "Sữa hạnh nhân", Quantity = 30, Category = "Sữa", Unit = "Lít", UnitPrice = 30000, ImportDate = new DateTime(2024, 2, 5), ExpirationDate = new DateTime(2024, 3, 10) },
            new Material { MaterialCode = "S004", MaterialName = "Sữa dừa", Quantity = 40, Category = "Sữa", Unit = "Lít", UnitPrice = 25000, ImportDate = new DateTime(2024, 1, 18), ExpirationDate = new DateTime(2024, 2, 15) },
            new Material { MaterialCode = "T001", MaterialName = "Trà xanh", Quantity = 70, Category = "Trà", Unit = "Kg", UnitPrice = 80000, ImportDate = new DateTime(2024, 1, 10), ExpirationDate = new DateTime(2024, 3, 1) },
            new Material { MaterialCode = "T002", MaterialName = "Trà đen", Quantity = 90, Category = "Trà", Unit = "Kg", UnitPrice = 75000, ImportDate = new DateTime(2024, 2, 12), ExpirationDate = new DateTime(2024, 4, 15) },
            new Material { MaterialCode = "S005", MaterialName = "Sữa yến mạch", Quantity = 25, Category = "Sữa", Unit = "Lít", UnitPrice = 35000, ImportDate = new DateTime(2024, 2, 8), ExpirationDate = new DateTime(2024, 3, 20) },
            new Material { MaterialCode = "C004", MaterialName = "Cafe Espresso", Quantity = 50, Category = "Cafe", Unit = "Kg", UnitPrice = 170000, ImportDate = new DateTime(2024, 1, 30), ExpirationDate = new DateTime(2024, 4, 10) },
            new Material { MaterialCode = "C005", MaterialName = "Cafe Latte", Quantity = 40, Category = "Cafe", Unit = "Kg", UnitPrice = 160000, ImportDate = new DateTime(2024, 2, 15), ExpirationDate = new DateTime(2024, 4, 25) },
            new Material { MaterialCode = "S006", MaterialName = "Sữa dâu", Quantity = 35, Category = "Sữa", Unit = "Lít", UnitPrice = 30000, ImportDate = new DateTime(2024, 2, 20), ExpirationDate = new DateTime(2024, 3, 30) },
            new Material { MaterialCode = "S007", MaterialName = "Sữa socola", Quantity = 45, Category = "Sữa", Unit = "Lít", UnitPrice = 28000, ImportDate = new DateTime(2024, 1, 25), ExpirationDate = new DateTime(2024, 3, 5) },
            new Material { MaterialCode = "T003", MaterialName = "Trà sữa Thái", Quantity = 50, Category = "Trà", Unit = "Kg", UnitPrice = 70000, ImportDate = new DateTime(2024, 2, 18), ExpirationDate = new DateTime(2024, 5, 1) },
            new Material { MaterialCode = "T004", MaterialName = "Trà hoa nhài", Quantity = 60, Category = "Trà", Unit = "Kg", UnitPrice = 85000, ImportDate = new DateTime(2024, 1, 12), ExpirationDate = new DateTime(2024, 3, 20) },
            new Material { MaterialCode = "T005", MaterialName = "Trà ô long", Quantity = 75, Category = "Trà", Unit = "Kg", UnitPrice = 90000, ImportDate = new DateTime(2024, 2, 2), ExpirationDate = new DateTime(2024, 4, 12) },
            new Material { MaterialCode = "S008", MaterialName = "Sữa tách béo", Quantity = 55, Category = "Sữa", Unit = "Lít", UnitPrice = 32000, ImportDate = new DateTime(2024, 1, 28), ExpirationDate = new DateTime(2024, 2, 25) },
            new Material { MaterialCode = "T006", MaterialName = "Trà cam thảo", Quantity = 65, Category = "Trà", Unit = "Kg", UnitPrice = 78000, ImportDate = new DateTime(2024, 1, 16), ExpirationDate = new DateTime(2024, 3, 8) },
            new Material { MaterialCode = "S009", MaterialName = "Sữa hạt điều", Quantity = 20, Category = "Sữa", Unit = "Lít", UnitPrice = 40000, ImportDate = new DateTime(2024, 2, 10), ExpirationDate = new DateTime(2024, 3, 22) }

        };
        mockUsers = new List<User>
        {
            new User { Username = "123", Password = "123" },
            new User { Username = "user2", Password = "pass2" },
            new User { Username = "admin", Password = "admin123" }
        };
    }
    public FullObservableCollection<Product> GetAllBeverage()
    {
        return new FullObservableCollection<Product>(_products);
    }
    public Category GetCategory(string category)
    {
        var categoryResult = _categories.FirstOrDefault(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
        return categoryResult ?? new Category
        (
            "Không có sản phẩm nào",
            new FullObservableCollection<Product>
            {
                new Product { Name = "Không có sản phẩm nào", Price = 0, Size = "M", Image = "Assets/no_product.jpg" }
            }
        );
    }

    public FullObservableCollection<Category> GetListTypeBeverage()
    {
        return new FullObservableCollection<Category>(_categories);
    }
    public int GetProductPrice(int beverageId, string size)
    {
        // Bảng giá dựa trên beverageId và kích thước sản phẩm
        var priceTable = new Dictionary<int, Dictionary<string, int>>
    {
        { 1, new Dictionary<string, int> { { "S", 40000 }, { "M", 45000 }, { "L", 50000 } } }, // Latte
        { 2, new Dictionary<string, int> { { "S", 30000 }, { "M", 35000 }, { "L", 38000 } } }, // Green Tea
        { 3, new Dictionary<string, int> { { "S", 28000 }, { "M", 30000 }, { "L", 35000 } } }, // Milk Tea
        { 4, new Dictionary<string, int> { { "S", 25000 }, { "M", 27000 }, { "L", 30000 } } }, // Orange Juice
        { 5, new Dictionary<string, int> { { "S", 40000 }, { "M", 45000 } } }, // Espresso
        { 6, new Dictionary<string, int> { { "S", 35000 }, { "M", 37000 }, { "L", 40000 } } }, // Black Coffee
        { 7, new Dictionary<string, int> { { "S", 32000 }, { "M", 35000 }, { "L", 38000 } } }, // Oolong Tea
        { 8, new Dictionary<string, int> { { "S", 27000 }, { "M", 29000 }, { "L", 31000 } } }, // Apple Juice
        { 9, new Dictionary<string, int> { { "M", 45000 }, { "L", 50000 } } }, // Smoothie
        { 10, new Dictionary<string, int> { { "S", 42000 }, { "M", 47000 }, { "L", 50000 } } }, // Iced Latte
        { 11, new Dictionary<string, int> { { "S", 43000 }, { "M", 48000 }, { "L", 51000 } } }, // Matcha Latte
        { 12, new Dictionary<string, int> { { "S", 28000 }, { "M", 32000 }, { "L", 35000 } } }, // Lemon Tea
        { 13, new Dictionary<string, int> { { "S", 27000 }, { "M", 29000 }, { "L", 31000 } } }, // Pineapple Juice
        { 14, new Dictionary<string, int> { { "M", 47000 }, { "L", 50000 } } }, // Mango Smoothie
        { 15, new Dictionary<string, int> { { "S", 42000 }, { "M", 45000 }, { "L", 48000 } } }  // Cappuccino
    };

        // Kiểm tra beverageId và kích thước trong bảng giá
        if (priceTable.ContainsKey(beverageId))
        {
            if (priceTable[beverageId].ContainsKey(size))
            {
                return priceTable[beverageId][size];  // Trả về giá nếu tìm thấy beverageId và kích thước
            }
            else
            {
                Console.WriteLine($"Kích thước '{size}' không có sẵn cho beverageId '{beverageId}'.");
                return -1;  // Trả về -1 nếu không tìm thấy kích thước
            }
        }
        else
        {
            Console.WriteLine($"BeverageId '{beverageId}' không có trong bảng giá.");
            return -1;  // Trả về -1 nếu không tìm thấy beverageId
        }
    }



    public FullObservableCollection<Invoice> GetPendingOrders()
    {
        return new FullObservableCollection<Invoice>(_pendingOrders);
    }

    public async Task<int> CreateOrder(Invoice invoice)
    {
        var newOrderId = _pendingOrders.Any() ? _pendingOrders.Max(order => order.InvoiceNumber) + 1 : 1;
        invoice.InvoiceNumber = newOrderId;
        invoice.CreatedTime = DateTime.Now;
        _pendingOrders.Add(invoice);
        await Task.Delay(50);
        return newOrderId;
    }

    public async Task AddOrderDetail(int orderId, InvoiceItem item)
    {
        var invoice = _pendingOrders.FirstOrDefault(o => o.InvoiceNumber == orderId);
        if (invoice != null)
        {
            
        }
        await Task.Delay(50);
    }
    public List<string> GetAllPaymentMethod()
    {
        return new() { "Tiền mặt", "Thẻ tín dụng", "Ví điện tử" };
    }
    public bool CompletePendingOrder(Invoice order)
    {
        var invoice = _pendingOrders.FirstOrDefault(o => o.InvoiceNumber == order.InvoiceNumber);
        if (invoice != null)
        {
            invoice.MarkAsPaid();
            _pendingOrders.Remove(invoice);
            return true;
        }
        return false;
    }

    public async Task<Revenue> GetRevenue(DateTime selectedDate)
    {
        await Task.Delay(10);

        return new Revenue
        {
            OrderCount = 81,
            TotalRevenue = 3551000,
            CashAmount = 2412000
        };
    }

    public async Task<List<TopProduct>> GetTopProducts(DateTime selectedDate)
    {
        await Task.Delay(100);

        return new List<TopProduct>
        {
            new() { ImageUrl = "/Assets/milk_tea.jpg", Name = "Milk tea", Revenue = 720000 },
            new() { ImageUrl = "/Assets/latte.jpg", Name = "Latte", Revenue = 675000 },
            new() { ImageUrl = "/Assets/oolong_tea.jpg", Name = "Oolong tea", Revenue = 630000 },
            new() { ImageUrl = "/Assets/black_coffee.jpg", Name = "Black coffee", Revenue = 518000 },
            new() { ImageUrl = "/Assets/espresso.jpg", Name = "Espresso", Revenue = 360000 },
            new() { ImageUrl = "/Assets/orange_juice.jpg", Name = "Orange juice", Revenue = 297000 },
        };

    }

    public async Task<List<TopCategory>> GetTopCategories(DateTime selectedDate)
    {
        await Task.Delay(100);

        return new List<TopCategory>
        {
            new() { ImageUrl = "/Assets/coffee.jpg", Name = "Coffee", Revenue = 878000 },
            new() { ImageUrl = "/Assets/tea.jpg", Name = "Tea", Revenue = 1350000 },
            new() { ImageUrl = "/Assets/juice.jpg", Name = "Juice", Revenue = 400000 },
            new() { ImageUrl = "/Assets/smoothie.jpg", Name = "Smoothie", Revenue = 120000 },
        };
    }

    public async Task<List<TopSeller>> GetTopSellers(DateTime selectedDate)
    {
        await Task.Delay(100);

        return new List<TopSeller>
        {
            new() { ImageUrl = "/Assets/milk_tea.jpg", Name = "Milk tea", Amount = 24 },
            new() { ImageUrl = "/Assets/oolong_tea.jpg", Name = "Oolong tea", Amount = 18 },
            new() { ImageUrl = "/Assets/latte.jpg", Name = "Latte", Amount = 15 },
            new() { ImageUrl = "/Assets/black_coffee.jpg", Name = "Black coffee", Amount = 14 },
            new() { ImageUrl = "/Assets/orange_juice.jpg", Name = "Orange juice", Amount = 11 },
            new() { ImageUrl = "/Assets/espresso.jpg", Name = "Espresso", Amount = 8 },
        };
    }

    private List<Material> mockMaterials;
    private List<User> mockUsers;
        

    // Implement ICoffeeShopDAO methods for Material
    public List<Material> GetAllMaterials() => mockMaterials;
    public Material GetMaterialByCode(string code) => mockMaterials.FirstOrDefault(m => m.MaterialCode == code);
    public bool AddMaterial(Material material)
    {
        if (mockMaterials.Any(m => m.MaterialCode == material.MaterialCode))
            return false;
        mockMaterials.Add(material);
        return true;
    }
    public bool UpdateMaterial(Material material)
    {
        var existingMaterial = GetMaterialByCode(material.MaterialCode);
        if (existingMaterial == null) return false;

        existingMaterial.MaterialName = material.MaterialName;
        existingMaterial.Quantity = material.Quantity;
        existingMaterial.Category = material.Category;
        existingMaterial.Unit = material.Unit;
        existingMaterial.UnitPrice = material.UnitPrice;
        existingMaterial.ImportDate = material.ImportDate;
        existingMaterial.ExpirationDate = material.ExpirationDate;
        return true;
    }
    public bool DeleteMaterial(string code)
    {
        var material = GetMaterialByCode(code);
        if (material == null) return false;
        mockMaterials.Remove(material);
        return true;
    }

    // Implement ICoffeeShopDAO methods for User
    public List<User> GetAllUsers() => mockUsers;
    public User GetUserByUsername(string username) => mockUsers.FirstOrDefault(u => u.Username == username);
    public bool AddUser(User user)
    {
        if (mockUsers.Any(u => u.Username == user.Username))
            return false;
        mockUsers.Add(user);
        return true;
    }
}
