using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Model;
using MySql.Data.MySqlClient;
using DotNetEnv;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.UI.Xaml.Controls.Primitives;
using Mysqlx.Notice;
using System.Data;

namespace App;

public class MySqlDao : IDao
{
    private readonly string _connectionString;

    public MySqlDao()
    {
        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\..\App\.env"));
        _connectionString = $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
                        $"port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                        $"database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
                        $"uid={Environment.GetEnvironmentVariable("DB_USER")};" +
                        $"pwd={Environment.GetEnvironmentVariable("DB_PASSWORD")};";
    }
    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
    public void ConnectToDatabase()
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                Console.WriteLine("Connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    // Query to retrieve data
    public List<Dictionary<string, object>> ExecuteSelectQuery(string query)
    {
        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        try
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return results;
    }

    public List<Dictionary<string, object>> ExecuteSelectQuery(string query, List<MySqlParameter> parameters)
    {
        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        try
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return results;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, List<MySqlParameter> parameters)
    {
        int rowsAffected = 0;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                await connection.OpenAsync();  // Sử dụng OpenAsync thay vì Open
                rowsAffected = await command.ExecuteNonQueryAsync();  // Sử dụng ExecuteNonQueryAsync
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return rowsAffected;
    }


    // Command to modify data (INSERT, UPDATE, DELETE)
    public int ExecuteNonQuery(string query, List<MySqlParameter> parameters)
    {
        int rowsAffected = 0;

        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return rowsAffected;
    }
    public Category GetCategory(string type)
    {

        var query = @"SELECT b.ID, b.BEVERAGE_NAME, b.IMAGE_PATH, tb.CATEGORY 
                          FROM BEVERAGE b
                          INNER JOIN TYPE_BEVERAGE tb ON b.CATEGORY_ID= tb.ID 
                          WHERE tb.CATEGORY = @type";
        var parameters = new List<MySqlParameter>
        {
            new ("@type", type)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var products = new List<Product>();

        foreach (var row in result)
        {
            products.Add(new Product
            {
                Id = Convert.ToInt32(row["ID"]),
                Name = row["BEVERAGE_NAME"].ToString(),
                Image = row["IMAGE_PATH"].ToString(),
                TypeBeverageId = Convert.ToInt32(row["ID"])
            });
        }

        return new Category(type, new FullObservableCollection<Product>(products));
    }

    public FullObservableCollection<Category> GetListTypeBeverage()
    {
        var query = "SELECT * FROM TYPE_BEVERAGE";
        var result = ExecuteSelectQuery(query);

        var categories = new FullObservableCollection<Category>();
        categories.Add(new Category("All", GetAllBeverage()));

        foreach (var row in result)
        {
            var type = row["CATEGORY"].ToString();
            categories.Add(new Category(type, GetCategory(type).Products));
        }

        return categories;
    }

    public FullObservableCollection<Product> GetAllBeverage()
    {
        var query = "SELECT * FROM BEVERAGE";
        var result = ExecuteSelectQuery(query);

        var products = new List<Product>();

        foreach (var row in result)
        {
            products.Add(new Product
            {
                Id = Convert.ToInt32(row["ID"]),
                Name = row["BEVERAGE_NAME"].ToString(),
                Image = row["IMAGE_PATH"].ToString(),
                TypeBeverageId = Convert.ToInt32(row["CATEGORY_ID"])
            });
        }

        return new FullObservableCollection<Product>(products);
    }

    public int GetProductPrice(int beverageId, string size)
    {
        var query = @"SELECT PRICE FROM BEVERAGE_SIZE WHERE BEVERAGE_ID = @id AND SIZE = @size";
        var parameters = new List<MySqlParameter>
        {
            new ("@id", beverageId),
            new ("@size", size)
        };

        var result = ExecuteSelectQuery(query, parameters);

        return result.Count > 0 ? Convert.ToInt32(result[0]["PRICE"]) : -1;
    }
    public string GetBeverageNameBySizeId(int beverageSizeId)
    {
        var query = @"
        SELECT b.BEVERAGE_NAME 
        FROM BEVERAGE b
        INNER JOIN BEVERAGE_SIZE bs ON b.ID = bs.BEVERAGE_ID
        WHERE bs.ID = @beverageSizeId";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@beverageSizeId", beverageSizeId)
        };

        var result = ExecuteSelectQuery(query, parameters);

        return result.Count > 0 ? result[0]["BEVERAGE_NAME"].ToString() : null;
    }

    private FullObservableCollection<InvoiceItem> GetOrderDetails(int orderId)
    {
        var query = "SELECT * FROM ORDER_DETAILS O JOIN BEVERAGE_SIZE BZ ON O.BEVERAGE_SIZE_ID = BZ.ID WHERE ORDER_ID = @orderId";
        var parameters = new List<MySqlParameter>
        {
            new ("@orderId", MySqlDbType.Int32) { Value = orderId }
        };

        var result = ExecuteSelectQuery(query, parameters);
        var items = new FullObservableCollection<InvoiceItem>();

        foreach (var row in result)
        {
            items.Add(new InvoiceItem
            {
                BeverageId = Convert.ToInt32(row["BEVERAGE_SIZE_ID"]),
                Name = GetBeverageNameBySizeId(Convert.ToInt32(row["BEVERAGE_SIZE_ID"])),
                Size = row["SIZE"].ToString(),
                Note = row["NOTE"].ToString(),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                Price = Convert.ToInt32(row["PRICE"]),
            });
        }

        return items;
    }
    public async Task<int> GetMaxAvailableQuantityAsync(int beverageSizeId)
    {
        string query = @"
            SELECT 
              FLOOR(MIN(MATERIAL.QUANTITY / RECIPE.QUANTITY)) AS MAX_BEVERAGE_COUNT
            FROM 
              RECIPE
            JOIN 
              MATERIAL ON RECIPE.MATERIAL_ID = MATERIAL.ID
            WHERE 
              RECIPE.BEVERAGE_SIZE_ID = @BeverageSizeId
            GROUP BY 
              RECIPE.BEVERAGE_SIZE_ID";

        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@BeverageSizeId", beverageSizeId)
        };

        var result = await ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public FullObservableCollection<Invoice> GetPendingOrders()
    {
        var query = "SELECT * FROM ORDERS WHERE COMPLETED_TIME IS NULL";
        var result = ExecuteSelectQuery(query);

        var orders = new FullObservableCollection<Invoice>();

        foreach (var row in result)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = Convert.ToInt32(row["ORDER_ID"]),
                TableNumber = row["RESERVED_TABLE_ID"] == DBNull.Value ? -1 : Convert.ToInt32(row["RESERVED_TABLE_ID"]),
                CreatedTime = Convert.ToDateTime(row["ORDER_TIME"]),
                PaymentMethod = row["PAYMENT_METHOD"].ToString(),
                InvoiceItems = GetOrderDetails(Convert.ToInt32(row["ORDER_ID"])),
            };
            invoice.EstimateTime = invoice.CreatedTime.AddMinutes(3 * invoice.TotalQuantity);
            orders.Add(invoice);
        }

        return orders;
    }
    public List<HistoryOrder> GetBeveragesPurchasedByCustomer(string phoneNumber)
    {
        var query = @"
        SELECT 
            B.ID,
            B.BEVERAGE_NAME,
            B.IMAGE_PATH,
            B.CATEGORY_ID,
            OD.QUANTITY,
            O.ORDER_TIME
        FROM 
            CUSTOMERS C
        JOIN 
            ORDERS O ON C.CUSTOMER_ID = O.CUSTOMER_ID
        JOIN 
            ORDER_DETAILS OD ON O.ORDER_ID = OD.ORDER_ID
        JOIN 
            BEVERAGE B ON OD.BEVERAGE_SIZE_ID = B.ID
        WHERE 
            C.PHONE_NUMBER = @phoneNumber
        ORDER BY 
            O.ORDER_TIME DESC;";

        var parameters = new List<MySqlParameter>
        {
            new ("@phoneNumber", phoneNumber)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var products = new List<HistoryOrder>();

        foreach (var row in result)
        {
            products.Add(new HistoryOrder
            {
                Id = Convert.ToInt32(row["ID"]),
                Name = row["BEVERAGE_NAME"].ToString(),
                Image = row["IMAGE_PATH"]?.ToString(),
                TypeBeverageId = Convert.ToInt32(row["CATEGORY_ID"]),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                OrderTime = Convert.ToDateTime(row["ORDER_TIME"])
            });
        }

        // Chuyển danh sách products thành JSON
        return products;
    }


    public async Task<object> ExecuteScalarAsync(string query, List<MySqlParameter> parameters)
    {
        using (var connection = new MySqlConnection(_connectionString))
        using (var command = new MySqlCommand(query, connection))
        {
            connection.Open();
            command.Parameters.AddRange(parameters.ToArray());
            return await command.ExecuteScalarAsync();
        }
    }
    public Task<List<string>> SuggestCustomerPhoneNumbers(string keyword)
    {
        var query = @"
        SELECT PHONE_NUMBER 
        FROM CUSTOMERS
        WHERE PHONE_NUMBER LIKE CONCAT('%', @keyword, '%')
        LIMIT 10";

        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@keyword", keyword)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var phoneNumbers = new List<string>();

        foreach (var row in result)
        {
            phoneNumbers.Add(row["PHONE_NUMBER"].ToString());
        }

        return Task.FromResult(phoneNumbers);

    }

    private async Task UpdateStockAfterOrder(Invoice invoice)
    {
        foreach (var item in invoice.InvoiceItems)
        {
            var beverageSizeId = GetBeverageSizeId(item.BeverageId, item.Size);

            // Giảm số lượng nguyên liệu trong kho
            var recipeQuery = @"
            SELECT MATERIAL_ID, QUANTITY
            FROM RECIPE
            WHERE BEVERAGE_SIZE_ID = @beverageSizeId";

            var recipeParams = new List<MySqlParameter>
        {
            new MySqlParameter("@beverageSizeId", beverageSizeId)
        };

            var recipeResults = ExecuteSelectQuery(recipeQuery, recipeParams);

            foreach (var row in recipeResults)
            {
                var materialId = Convert.ToInt32(row["MATERIAL_ID"]);
                var quantityUsed = Convert.ToInt32(row["QUANTITY"]) * item.Quantity;

                // Cập nhật kho (giảm số lượng nguyên liệu)
                var updateStockQuery = @"
                UPDATE MATERIAL
                SET QUANTITY = QUANTITY - @quantityUsed
                WHERE ID = @materialId";

                var updateStockParams = new List<MySqlParameter>
            {
                new MySqlParameter("@quantityUsed", quantityUsed),
                new MySqlParameter("@materialId", materialId)
            };

                await ExecuteNonQueryAsync(updateStockQuery, updateStockParams);
            }
        }
    }


    public async Task<int> CreateOrder(Invoice invoice)
    {

        // Câu truy vấn SQL để tạo đơn hàng
        var query = @"
                        INSERT INTO ORDERS (TOTAL_AMOUNT, ORDER_TIME, PAYMENT_METHOD, CONSUMED_POINTS, AMOUNT_DUE, EMPLOYEE_ID) 
                        VALUES (@total, @time, @method, @points, @amountDue, @employee);
                        SELECT LAST_INSERT_ID();";
        var parameters = new List<MySqlParameter>
        {
            new ("@total", invoice.TotalPrice),
            new ("@time", invoice.CreatedTime),
            new ("@points", invoice.ConsumedPoints),
            new ("@amountDue", invoice.AmountDue),
            new ("@method", invoice.PaymentMethod),
            new ("@employee", 1),
        };

        var orderId = Convert.ToInt32(await ExecuteScalarAsync(query, parameters));

        // Cập nhật kho sau khi tạo đơn hàng
        await UpdateStockAfterOrder(invoice);

        return orderId;
    }

    public int GetBeverageSizeId(int beverageId, string size)
    {
        var query = "SELECT ID FROM BEVERAGE_SIZE WHERE BEVERAGE_ID = @beverageId AND SIZE = @size";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@beverageId", beverageId),
            new MySqlParameter("@size", size)
        };

        var result = ExecuteSelectQuery(query, parameters);

        return result.Count > 0 ? Convert.ToInt32(result[0]["ID"]) : -1;
    }

    public async Task AddOrderDetail(int orderId, InvoiceItem item)
    {
        var query = "INSERT INTO ORDER_DETAILS (ORDER_ID, BEVERAGE_SIZE_ID, QUANTITY, PRICE, SUBTOTAL, NOTE) VALUES (@orderId, @beverageSizeId, @quantity, @price, @total, @note)";
        var parameters = new List<MySqlParameter>
        {
            new ("@orderId", orderId),
            new ("@beverageSizeId", MySqlDbType.Int32) { Value = GetBeverageSizeId(item.BeverageId, item.Size) },
            new ("@quantity", item.Quantity),
            new ("@price", item.Price),
            new ("@total", item.Total),
            new ("@note", item.Note)

        };

        ExecuteNonQuery(query, parameters);
    }

    public bool CompletePendingOrder(Invoice order)
    {
        var query = "UPDATE ORDERS SET COMPLETED_TIME = @time WHERE ORDER_ID = @invoice";
        var parameters = new List<MySqlParameter>
        {
            new ("@invoice", order.InvoiceNumber),
            new ("@time", order.CompleteTime)
        };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public List<string> GetAllPaymentMethod()
    {
        var query = "SELECT METHOD_NAME FROM PAYMENT_METHOD";
        var result = ExecuteSelectQuery(query);

        var methods = new List<string>();

        foreach (var row in result)
        {
            methods.Add(row["METHOD_NAME"].ToString());
        }

        return methods;
    }
    public int GetComsumedPoints(string phoneNumber)
    {
        var query = "SELECT POINTS FROM CUSTOMERS WHERE PHONE_NUMBER = @phone";
        var parameters = new List<MySqlParameter>
        {
            new("@phone", phoneNumber)
        };

        var result = ExecuteSelectQuery(query, parameters);

        return result.Count > 0 ? Convert.ToInt32(result[0]["POINTS"]) : 0;
    }

    public void ConsumePoints(int points, string customerPhone)
    {
        try
        {
            // Lấy số điểm hiện tại của khách hàng
            int currentPoints = GetComsumedPoints(customerPhone);

            // Tính số điểm mới sau khi cộng thêm
            int newPoints = currentPoints - points;

            // Cập nhật số điểm mới vào bảng CUSTOMERS
            var query = "UPDATE CUSTOMERS SET POINTS = @newPoints WHERE PHONE_NUMBER = @phone";
            var parameters = new List<MySqlParameter>
        {
            new ("@newPoints", newPoints),
            new ("@phone", customerPhone)
        };

            // Thực hiện cập nhật vào cơ sở dữ liệu
            ExecuteNonQuery(query, parameters);

            Console.WriteLine($"Points successfully added for customer {customerPhone}. Total points: {newPoints}");
        }
        catch (Exception ex)
        {
            // Xử lý lỗi nếu có
            Console.WriteLine($"An error occurred while adding points: {ex.Message}");
        }
    }
    public void BonusPoints(int amountDue, string customerPhone)
    {
        try
        {
            // Chuyển đổi số tiền AmountDue sang giá trị điểm (1 điểm = 1 VND, hoặc theo tỷ lệ khác nếu cần)
            int pointsToAdd = Convert.ToInt32(amountDue * 0.1);  // Có thể điều chỉnh tỷ lệ tại đây (ví dụ: 1 điểm = 1 VND)

            // Lấy số điểm hiện tại của khách hàng
            int currentPoints = GetComsumedPoints(customerPhone);

            // Tính số điểm mới sau khi cộng thêm
            int newPoints = currentPoints + pointsToAdd;

            // Cập nhật số điểm mới vào bảng CUSTOMERS
            var query = "UPDATE CUSTOMERS SET POINTS = @newPoints WHERE PHONE_NUMBER = @phone";
            var parameters = new List<MySqlParameter>
        {
            new ("@newPoints", newPoints),
            new ("@phone", customerPhone)
        };

            // Thực hiện cập nhật vào cơ sở dữ liệu
            ExecuteNonQuery(query, parameters);

            Console.WriteLine($"Points successfully added for customer {customerPhone}. Total points: {newPoints}");
        }
        catch (Exception ex)
        {
            // Xử lý lỗi nếu có
            Console.WriteLine($"An error occurred while adding points: {ex.Message}");
        }
    }
    public (FullObservableCollection<Customer> Customers, int TotalCount) GetCustomers(int pageNumber, int pageSize, string? name = null, string? phoneNumber = null, int? minPoints = null, int? maxPoints = null)
    {
        int offset = (pageNumber - 1) * pageSize;
        var query = @"
            SELECT COUNT(*) OVER() AS TotalCount, CUSTOMER_ID, CUSTOMER_NAME, PHONE_NUMBER, EMAIL, POINTS
            FROM CUSTOMERS
            WHERE 1=1";

        var parameters = new List<MySqlParameter>();

        if (!string.IsNullOrEmpty(name))
        {
            query += " AND CUSTOMER_NAME LIKE @Name";
            parameters.Add(new MySqlParameter("@Name", $"%{name}%"));
        }

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            query += " AND PHONE_NUMBER LIKE @PhoneNumber";
            parameters.Add(new MySqlParameter("@PhoneNumber", $"%{phoneNumber}%"));
        }
        if (minPoints.HasValue)
        {
            query += " AND POINTS >= @MinPoints";
            parameters.Add(new MySqlParameter("@MinPoints", minPoints));
        }

        if (maxPoints.HasValue)
        {
            query += " AND POINTS <= @MaxPoints";
            parameters.Add(new MySqlParameter("@MaxPoints", maxPoints));
        }

        query += " LIMIT @PageSize OFFSET @Offset";
        parameters.Add(new MySqlParameter("@PageSize", pageSize));
        parameters.Add(new MySqlParameter("@Offset", offset));

        var result = ExecuteSelectQuery(query, parameters);

        var customers = new FullObservableCollection<Customer>();
        int totalCount = 0;

        foreach (var row in result)
        {
            if (totalCount == 0)
            {
                totalCount = Convert.ToInt32(row["TotalCount"]);
            }

            var customer = new Customer
            {
                CustomerId = Convert.ToInt32(row["CUSTOMER_ID"]),
                CustomerName = row["CUSTOMER_NAME"].ToString(),
                PhoneNumber = row["PHONE_NUMBER"].ToString(),
                Email = row["EMAIL"].ToString(),
                Points = Convert.ToInt32(row["POINTS"])
            };
            customers.Add(customer);
        }

        return (customers, totalCount);
    }
    public bool AddCustomer(Customer customer)
    {
        var query = @"
        INSERT INTO CUSTOMERS (CUSTOMER_NAME, PHONE_NUMBER, EMAIL, POINTS)
        VALUES (@CustomerName, @PhoneNumber, @Email, @Points)";

        var parameters = new List<MySqlParameter>
        {
            new ("@CustomerName", customer.CustomerName),
            new ("@PhoneNumber", customer.PhoneNumber ?? (object)DBNull.Value),
            new ("@Email", customer.Email ?? (object)DBNull.Value),
            new ("@Points", customer.Points)
        };

        int rowsAffected = ExecuteNonQuery(query, parameters);
        return rowsAffected > 0;
    }
    public bool UpdateCustomer(Customer customer)
    {
        var query = @"
        UPDATE CUSTOMERS
        SET CUSTOMER_NAME = @CustomerName,
            PHONE_NUMBER = @PhoneNumber,
            EMAIL = @Email,
            POINTS = @Points
        WHERE CUSTOMER_ID = @CustomerId";

        var parameters = new List<MySqlParameter>
        {
            new ("@CustomerName", customer.CustomerName),
            new ("@PhoneNumber", customer.PhoneNumber ?? (object)DBNull.Value),
            new ("@Email", customer.Email ?? (object)DBNull.Value),
            new ("@Points", customer.Points),
            new ("@CustomerId", customer.CustomerId)
        };

        int rowsAffected = ExecuteNonQuery(query, parameters);
        return rowsAffected > 0;
    }
    public bool DeleteCustomer(int customerId)
    {
        var query = @"
        DELETE FROM CUSTOMERS
        WHERE CUSTOMER_ID = @CustomerId";

        var parameters = new List<MySqlParameter>
        {
            new ("@CustomerId", customerId)
        };

        int rowsAffected = ExecuteNonQuery(query, parameters);
        return rowsAffected > 0;
    }


    public List<Material> GetAllMaterials()
    {
        var query = "SELECT * FROM MATERIAL";
        var result = ExecuteSelectQuery(query);

        var materials = new List<Material>();

        foreach (var row in result)
        {
            materials.Add(new Material
            {
                MaterialCode = row["MATERIAL_CODE"].ToString(),
                MaterialName = row["MATERIAL_NAME"].ToString(),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                Category = row["CATEGORY"].ToString(),
                Unit = row["UNIT"].ToString(),
                UnitPrice = Convert.ToInt32(row["UNIT_PRICE"]),
                ImportDate = Convert.ToDateTime(row["IMPORT_DATE"]),
                ExpirationDate = Convert.ToDateTime(row["EXPIRATION_DATE"]),
                Threshold = Convert.ToInt32(row["NOTIFYCATION_THRESHOLD"])
            });
        }

        return materials;
    }
    
    public List<Material> getAllThreshold()
    {
        var query = "SELECT * FROM MATERIAL";
        var result = ExecuteSelectQuery(query);

        var materials = new List<Material>();

        foreach (var row in result)
        {
            materials.Add(new Material
            {
                MaterialCode = row["MATERIAL_CODE"].ToString(),
                MaterialName = row["MATERIAL_NAME"].ToString(),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                Category = row["CATEGORY"].ToString(),
                Unit = row["UNIT"].ToString(),
                UnitPrice = Convert.ToInt32(row["UNIT_PRICE"]),
                ImportDate = Convert.ToDateTime(row["IMPORT_DATE"]),
                ExpirationDate = Convert.ToDateTime(row["EXPIRATION_DATE"]),
                Threshold = Convert.ToInt32(row["NOTIFYCATION_THRESHOLD"])
            });
        }

        return materials;

    }
    public Material GetMaterialByCode(string code)
    {
        var query = "SELECT * FROM MATERIAL WHERE MATERIAL_CODE = @code";
        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@code", code)
    };

        var result = ExecuteSelectQuery(query, parameters);

        if (result.Count == 0) return null;

        var row = result[0];
        return new Material
        {
            MaterialCode = row["MATERIAL_CODE"].ToString(),
            MaterialName = row["MATERIAL_NAME"].ToString(),
            Quantity = Convert.ToInt32(row["QUANTITY"]),
            Category = row["CATEGORY"].ToString(),
            Unit = row["UNIT"].ToString(),
            UnitPrice = Convert.ToInt32(row["UNIT_PRICE"]),
            ImportDate = Convert.ToDateTime(row["IMPORT_DATE"]),
            ExpirationDate = Convert.ToDateTime(row["EXPIRATION_DATE"])
        };
    }

    public bool AddMaterial(Material material)
    {
        var query = "INSERT INTO MATERIAL (MATERIAL_CODE, MATERIAL_NAME, QUANTITY, CATEGORY, UNIT, UNIT_PRICE, IMPORT_DATE, EXPIRATION_DATE) VALUES (@code, @name, @quantity, @category, @unit, @unitPrice, @importDate, @expirationDate)";
        var parameters = new List<MySqlParameter>
    {
        new ("@code", material.MaterialCode),
        new ("@name", material.MaterialName),
        new ("@quantity", material.Quantity),
        new ("@category", material.Category),
        new ("@unit", material.Unit),
        new ("@unitPrice", material.UnitPrice),
        new ("@importDate", material.ImportDate),
        new ("@expirationDate", material.ExpirationDate)
    };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public bool UpdateMaterial(Material material)
    {
        var query = "UPDATE MATERIAL SET MATERIAL_NAME = @name, QUANTITY = @quantity, CATEGORY = @category, UNIT = @unit, UNIT_PRICE = @unitPrice, IMPORT_DATE = @importDate, EXPIRATION_DATE = @expirationDate WHERE MATERIAL_CODE = @code";
        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@code", material.MaterialCode),
        new MySqlParameter("@name", material.MaterialName),
        new MySqlParameter("@quantity", material.Quantity),
        new MySqlParameter("@category", material.Category),
        new MySqlParameter("@unit", material.Unit),
        new MySqlParameter("@unitPrice", material.UnitPrice),
        new MySqlParameter("@importDate", material.ImportDate),
        new MySqlParameter("@expirationDate", material.ExpirationDate)
    };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public bool DeleteMaterial(string code)
    {
        var query = "DELETE FROM MATERIAL WHERE MATERIAL_CODE = @code";
        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@code", code)
    };

        return ExecuteNonQuery(query, parameters) > 0;
    }














    public List<User> GetAllUsers()
    {
        var query = "SELECT * FROM USERS";
        var result = ExecuteSelectQuery(query);

        var users = new List<User>();

        foreach (var row in result)
        {
            users.Add(new User
            {
                Username = row["USERNAME"].ToString(),
                Password = row["USER_PASSWORD"].ToString()
            });
        }

        return users;
    }

    public User GetUserByUsername(string username)
    {
        var query = "SELECT USERNAME, USER_PASSWORD, EMAIL, EMPLOYEE_ID FROM ACCOUNT WHERE USERNAME = @username";
        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@username", username)
    };

        var result = ExecuteSelectQuery(query, parameters);

        if (result.Count == 0) return null;

        var row = result[0];
        return new User
        {
            Id = Convert.ToInt32(row["EMPLOYEE_ID"]),
            Username = row["USERNAME"].ToString(),
            Password = row["USER_PASSWORD"].ToString(),
            Email = row["EMAIL"]?.ToString() // Ánh xạ email từ kết quả
        };
    }


    public bool AddUser(User user)
    {
        var query = "INSERT INTO USERS (USERNAME, PASSWORD) VALUES (@username, @password)";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@username", user.Username),
            new MySqlParameter("@password", user.Password)
        };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public async Task<Revenue> GetRevenue(DateTime selectedDate)
    {
        var query = @"
        SELECT 
            SUM(o.TOTAL_AMOUNT) AS TotalRevenue, 
            COUNT(o.ORDER_ID) AS OrderCount,
            SUM(CASE WHEN o.PAYMENT_METHOD = 'Cash' THEN o.TOTAL_AMOUNT ELSE 0 END) AS CashAmount
        FROM 
            ORDERS o
        WHERE 
            DATE(o.ORDER_TIME) = @selectedDate";

        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@selectedDate", selectedDate)
        };

        var result = ExecuteSelectQuery(query, parameters);

        if (result.Count > 0)
        {
            var row = result[0];
            return new Revenue
            {
                TotalRevenue = row["TotalRevenue"] != DBNull.Value ? Convert.ToInt32(row["TotalRevenue"]) : 0,
                OrderCount = row["OrderCount"] != DBNull.Value ? Convert.ToInt32(row["OrderCount"]) : 0,
                CashAmount = row["CashAmount"] != DBNull.Value ? Convert.ToInt32(row["CashAmount"]) : 0
            }; 
        }

        return new Revenue();
    }
    public async Task<List<TopProduct>> GetTopProducts(DateTime selectedDate)
    {
        var query = @"
        SELECT
            b.IMAGE_PATH AS ImageUrl,
            b.BEVERAGE_NAME AS Name,
            SUM(od.SUBTOTAL) AS Revenue
        FROM
            ORDER_DETAILS od
        JOIN
            BEVERAGE_SIZE bz ON od.BEVERAGE_SIZE_ID = bz.ID
        JOIN 
            BEVERAGE b ON bz.BEVERAGE_ID = b.ID 
        JOIN
            ORDERS o ON od.ORDER_ID = o.ORDER_ID
        WHERE
            DATE(o.ORDER_TIME) = @selectedDate
        GROUP BY
            b.IMAGE_PATH, b.BEVERAGE_NAME
        ORDER BY
            Revenue DESC
        LIMIT 5";

        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@selectedDate", selectedDate)
    };

        var result = ExecuteSelectQuery(query, parameters);

        var topProducts = new List<TopProduct>();

        foreach (var row in result)
        {
            topProducts.Add(new TopProduct
            {
                ImageUrl = row["ImageUrl"].ToString(),
                Name = row["Name"].ToString(),
                Revenue = Convert.ToInt32(row["Revenue"])
            });
        }

        return topProducts;
    }

    public async Task<List<TopCategory>> GetTopCategories(DateTime selectedDate)
    {
        var query = @"
        SELECT
            t.IMAGE_PATH AS ImageUrl,
            t.CATEGORY AS Name,
            SUM(od.SUBTOTAL) AS Revenue
        FROM
            ORDER_DETAILS od
        JOIN
            BEVERAGE_SIZE bz ON od.BEVERAGE_SIZE_ID = bz.ID
        JOIN 
            BEVERAGE b ON bz.BEVERAGE_ID = b.ID 
        JOIN
            TYPE_BEVERAGE t ON b.CATEGORY_ID = t.ID
        JOIN
            ORDERS o ON od.ORDER_ID = o.ORDER_ID
        WHERE
            DATE(o.ORDER_TIME) = @selectedDate
        GROUP BY
            t.CATEGORY, t.IMAGE_PATH
        ORDER BY
            Revenue DESC
        LIMIT 5";


        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@selectedDate", selectedDate)
    };

        var result = ExecuteSelectQuery(query, parameters);

        var topCategories = new List<TopCategory>();

        foreach (var row in result)
        {
            topCategories.Add(new TopCategory
            {
                ImageUrl = row["ImageUrl"].ToString(),
                Name = row["Name"].ToString(),
                Revenue = Convert.ToInt32(row["Revenue"])
            });
        }

        return topCategories;
    }

    public async Task<List<TopSeller>> GetTopSellers(DateTime selectedDate)
    {
        var query = @"
        SELECT 
            b.IMAGE_PATH AS ImageUrl, 
            b.BEVERAGE_NAME AS Name,
            SUM(od.QUANTITY) AS Amount
        FROM
            ORDER_DETAILS od
        JOIN
            BEVERAGE_SIZE bz ON od.BEVERAGE_SIZE_ID = bz.ID
        JOIN 
            BEVERAGE b ON bz.BEVERAGE_ID = b.ID 
        JOIN
            ORDERS o ON od.ORDER_ID = o.ORDER_ID
        WHERE
            DATE(o.ORDER_TIME) = @selectedDate
        GROUP BY 
            b.IMAGE_PATH, b.BEVERAGE_NAME
        ORDER BY 
            Amount DESC
        LIMIT 5";


        var parameters = new List<MySqlParameter>
        {
            new ("@selectedDate", selectedDate)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var topSellers = new List<TopSeller>();

        foreach (var row in result)
        {
            topSellers.Add(new TopSeller
            {
                ImageUrl = row["ImageUrl"].ToString(),
                Name = row["Name"].ToString(),
                Amount = Convert.ToInt32(row["Amount"])
            });
        }

        return topSellers;
    }
    public bool UpdateMaterialThreshold(string materialCode, int newThreshold)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            var query = @"UPDATE MATERIAL SET NOTIFYCATION_THRESHOLD = @Threshold WHERE MATERIAL_CODE = @MaterialCode";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.Add(new MySqlParameter("@Threshold", newThreshold));
            command.Parameters.Add(new MySqlParameter("@MaterialCode", materialCode));
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating material threshold: {ex.Message}");
            return false;
        }
    }



    public List<Material> GetAllMaterialsOutStock()
    {
        var query = "SELECT * FROM MATERIAL WHERE QUANTITY < NOTIFYCATION_THRESHOLD";
        var result = ExecuteSelectQuery(query);

        var materials = new List<Material>();

        foreach (var row in result)
        {
            materials.Add(new Material
            {
                MaterialCode = row["MATERIAL_CODE"].ToString(),
                MaterialName = row["MATERIAL_NAME"].ToString(),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                Category = row["CATEGORY"].ToString(),
                Unit = row["UNIT"].ToString(),
                UnitPrice = Convert.ToInt32(row["UNIT_PRICE"]),
                ImportDate = Convert.ToDateTime(row["IMPORT_DATE"]),
                ExpirationDate = Convert.ToDateTime(row["EXPIRATION_DATE"]),
                Threshold = Convert.ToInt32(row["NOTIFYCATION_THRESHOLD"])
            });
        }

        return materials;
    }

    public bool UpdateUser(User user)
    {
        var query = @"
    UPDATE ACCOUNT
    SET USER_PASSWORD = @Password
    WHERE USERNAME = @Username";

        var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@Username", user.Username),
        new MySqlParameter("@Password", user.Password) // Nếu cần mã hóa, mã hóa trước khi lưu
    };

        return ExecuteNonQuery(query, parameters) > 0;
    }
    public List<ShiftAttendance> GetShiftAttendances(DateTime startDate, DateTime endDate)
    {
        var shiftAttendances = new List<ShiftAttendance>();

        string query = @"
        SELECT sa.ID, sa.EMPLOYEE_ID, a.EMP_NAME, sa.SHIFT_DATE, sa.MORNING_SHIFT, sa.AFTERNOON_SHIFT, sa.NOTE, sa.CREATED_AT, sa.UPDATED_AT
        FROM SHIFT_ATTENDANCE sa
        JOIN ACCOUNT a ON sa.EMPLOYEE_ID = a.EMPLOYEE_ID
        WHERE sa.SHIFT_DATE BETWEEN @StartDate AND @EndDate";

        var parameters = new List<MySqlParameter>
        {
            new ("@StartDate", startDate),
            new ("@EndDate", endDate)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var shiftAttendanceDict = new Dictionary<int, ShiftAttendance>();

        foreach (var row in result)
        {
            int employeeId = Convert.ToInt32(row["EMPLOYEE_ID"]);
            if (!shiftAttendanceDict.ContainsKey(employeeId))
            {
                var shiftAttendance = new ShiftAttendance
                {
                    Id = Convert.ToInt32(row["ID"]),
                    EmployeeId = employeeId,
                    Name = row["EMP_NAME"].ToString(),
                    Shifts = new FullObservableCollection<Shift>()
                };
                shiftAttendanceDict[employeeId] = shiftAttendance;
            }

            var shift = new Shift
            {
                ShiftDate = Convert.ToDateTime(row["SHIFT_DATE"]),
                MorningShift = Convert.ToBoolean(row["MORNING_SHIFT"]),
                AfternoonShift = Convert.ToBoolean(row["AFTERNOON_SHIFT"]),
                Note = row["NOTE"].ToString(),
                CreatedAt = Convert.ToDateTime(row["CREATED_AT"]),
                UpdatedAt = Convert.ToDateTime(row["UPDATED_AT"])
            };

            shiftAttendanceDict[employeeId].Shifts.Add(shift);
        }

        shiftAttendances.AddRange(shiftAttendanceDict.Values);

        return shiftAttendances;
    }
    // New method to add shift attendance
    public async Task<bool> AddShiftAttendance(Shift shift, int employeeId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();

            // Determine if the current time is morning or afternoon
            bool isAfternoon = DateTime.Now.TimeOfDay >= new TimeSpan(12, 0, 0);

            // Check if the employee has already worked the shift on the same day
            string checkQuery = "SELECT COUNT(*) FROM SHIFT_ATTENDANCE WHERE EMPLOYEE_ID = @EmployeeId AND SHIFT_DATE = @ShiftDate";
            var checkParameters = new List<MySqlParameter>
            {
                new     ("@EmployeeId", employeeId),
                new     ("@ShiftDate", shift.ShiftDate)
            };

            var count = (long)await ExecuteScalarAsync(checkQuery, checkParameters);

            if (count > 0)
            {
                // Update the existing record to include the current shift
                string updateQuery = isAfternoon
                    ? "UPDATE SHIFT_ATTENDANCE SET AFTERNOON_SHIFT = @AfternoonShift WHERE EMPLOYEE_ID = @EmployeeId AND SHIFT_DATE = @ShiftDate"
                    : "UPDATE SHIFT_ATTENDANCE SET MORNING_SHIFT = @MorningShift WHERE EMPLOYEE_ID = @EmployeeId AND SHIFT_DATE = @ShiftDate";

                var updateParameters = new List<MySqlParameter>
                {
                    new     (isAfternoon ? "@AfternoonShift" : "@MorningShift", isAfternoon ? shift.AfternoonShift : shift.MorningShift),
                    new     ("@EmployeeId", employeeId),
                    new     ("@ShiftDate", shift.ShiftDate)
                };

                int result = await ExecuteNonQueryAsync(updateQuery, updateParameters);
                return result > 0;
            }
            else
            {
                // Insert a new record for the shift
                string insertQuery = "INSERT INTO SHIFT_ATTENDANCE (EMPLOYEE_ID, SHIFT_DATE, MORNING_SHIFT, AFTERNOON_SHIFT) VALUES (@EmployeeId, @ShiftDate, @MorningShift, @AfternoonShift)";
                var insertParameters = new List<MySqlParameter>
                {
                    new     ("@EmployeeId", employeeId),
                    new     ("@ShiftDate", shift.ShiftDate),
                    new     ("@MorningShift", isAfternoon ? false : shift.MorningShift),
                    new     ("@AfternoonShift", isAfternoon ? shift.AfternoonShift : false)
                };

                int result = await ExecuteNonQueryAsync(insertQuery, insertParameters);
                return result > 0;
            }
        }
    }
    public async Task<Dictionary<int, double>> GetWorkingHoursForMonth(int month, int year)
    {
        var workingHours = new Dictionary<int, double>();

        string query = @"
            SELECT EMPLOYEE_ID, SHIFT_DATE, MORNING_SHIFT, AFTERNOON_SHIFT
            FROM SHIFT_ATTENDANCE
            WHERE MONTH(SHIFT_DATE) = @Month AND YEAR(SHIFT_DATE) = @Year";

        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", year);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int employeeId = reader.GetInt32("EMPLOYEE_ID");
                        DateTime shiftDate = reader.GetDateTime("SHIFT_DATE");
                        bool morningShift = reader.GetBoolean("MORNING_SHIFT");
                        bool afternoonShift = reader.GetBoolean("AFTERNOON_SHIFT");

                        double hoursWorked = 0;
                        if (morningShift)
                        {
                            hoursWorked += 4.5; // Giả sử ca sáng là 4.5 giờ
                        }
                        if (afternoonShift)
                        {
                            hoursWorked += 4.95; // Giả sử ca chiều là 4.95 giờ
                        }

                        if (workingHours.ContainsKey(employeeId))
                        {
                            workingHours[employeeId] += hoursWorked;
                        }
                        else
                        {
                            workingHours[employeeId] = hoursWorked;
                        }
                    }
                }
            }
        }

        return workingHours;
    }
}



