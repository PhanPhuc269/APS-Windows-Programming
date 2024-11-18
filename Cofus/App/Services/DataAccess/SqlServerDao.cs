using System;
using System.Collections.Generic;
using Npgsql;
using App.Model;
using App;

namespace DemoListBinding1610;

public class SqlServerDao
{
    private readonly string _connectionString = """
            Host=localhost;
            Database=mystore;
            Username=postgres;
            Password=1234;
        """;

    // Lấy thông tin Category theo loại
    public Category GetCategory(string category)
    {

        var categoryResult = new Category(category, new FullObservableCollection<Product>());
       

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var sql = @"
                SELECT tb.""ID"" AS CategoryID, tb.""CATEGORY"", b.""ID"" AS BeverageID, b.""BEVERAGE_NAME"", b.""SIZE"", b.""PRICE"", b.""IMAGE_PATH""
                FROM ""TYPE_BEVERAGE"" tb
                JOIN ""BEVERAGE"" b ON tb.""ID"" = b.""CATEGORY_ID""
                WHERE tb.""CATEGORY"" = @Category";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Category", category);
        command.Parameters["@Category"].NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar;
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // Thêm từng sản phẩm vào danh sách Products
            var product = new Product
            {
                Id = (int)reader["BeverageID"],
                Name = (string)reader["BEVERAGE_NAME"],
                Price = (int)reader["PRICE"],
                Size = (string)reader["SIZE"],
                Image = (string)reader["IMAGE_PATH"]
            };
            categoryResult.Products.Add(product);
        }
        int count = categoryResult.Products.Count;

        return count > 0 ? categoryResult : new Category ( "Không có sản phẩm nào", new FullObservableCollection<Product>()
            {
                new Product { Name = "Không có sản phẩm nào", Price = 0, Size = "M", Image="Assets/soup_paris.jpg"}
            } 
        );
    }

    // Lấy danh sách loại thức uống
    public FullObservableCollection<Category> GetListTypeBeverage()
    {
        var result = new FullObservableCollection<Category>();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var sql = "SELECT * FROM \"TYPE_BEVERAGE\"";
        using var command = new NpgsqlCommand(sql, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var name = (string)reader["CATEGORY"];
            // ServiceFactory.GetChildOf(typeof(IDao)) as IDao
            var typeBeverage = new Category(name, GetCategory(name).Products);
            result.Add(typeBeverage);
        }

        return result;
    }

    // Lấy danh sách hóa đơn chờ xử lý
    public FullObservableCollection<Invoice> GetPendingOrders()
    {
        var result = new FullObservableCollection<Invoice>();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var sql = "SELECT * FROM \"ORDERS\" WHERE \"COMPLETED_TIME\" IS NULL";
        using var command = new NpgsqlCommand(sql, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var invoice = new Invoice
            {
                InvoiceNumber = (int)reader["ORDER_ID"],
                TableNumber = (int)reader["RESERVED_TABLE_ID"],
                CreatedTime = (DateTime)reader["ORDER_TIME"],
            };
            result.Add(invoice);
        }

        return result;
    }
    public async Task<int> CreateOrder(Invoice invoice)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            int orderId;
            var sql = @"
                INSERT INTO ""ORDERS"" (""CUSTOMER_ID"", ""ORDER_TIME"", ""PAYMENT_METHOD_ID"", ""TOTAL_AMOUNT"") 
                VALUES (@customerId, @orderTime, @payMethod, @totalAmount) 
                RETURNING ""ORDER_ID"";";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@customerId", 1); // invoice.CustomerId
                cmd.Parameters.AddWithValue("@orderTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@totalAmount", invoice.TotalPrice);
                cmd.Parameters.AddWithValue("@payMethod", 1); // invoice.PaymentMethodId   

                orderId = (int)await cmd.ExecuteScalarAsync();
            }

            return orderId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating the order: {ex.Message}");
            throw;
        }
    }

    public async Task AddOrderDetail(int orderId, InvoiceItem item)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using (var cmd = new NpgsqlCommand(@"INSERT INTO ""ORDER_DETAILS"" (""ORDER_ID"", ""BEVERAGE_ID"", ""QUANTITY"", ""PRICE"", ""SUBTOTAL"") VALUES (@orderId, @beverageId, @quantity, @price, @subtotal);", connection))
        {
            cmd.Parameters.AddWithValue("orderId", orderId);
            cmd.Parameters.AddWithValue("beverageId", item.BeverageId);
            cmd.Parameters.AddWithValue("quantity", item.Quantity);
            cmd.Parameters.AddWithValue("price", item.Price);
            cmd.Parameters.AddWithValue("subtotal", item.Total);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
