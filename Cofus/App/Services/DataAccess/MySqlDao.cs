﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Model;
using MySql.Data.MySqlClient;
using DotNetEnv;
using Microsoft.UI.Xaml.Controls.Primitives;

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

    public FullObservableCollection<Invoice> GetPendingOrders()
    {
        var query = "SELECT * FROM ORDERS WHERE COMPLETED_TIME IS NULL";
        var result = ExecuteSelectQuery(query);

        var orders = new FullObservableCollection<Invoice>();

        foreach (var row in result)
        {
            orders.Add(new Invoice
            {
                InvoiceNumber = Convert.ToInt32(row["ORDER_ID"]),
                TableNumber = row["RESERVED_TABLE_ID"] == DBNull.Value ? -1 : Convert.ToInt32(row["RESERVED_TABLE_ID"]),
                CreatedTime = Convert.ToDateTime(row["ORDER_TIME"]),
                PaymentMethod = row["PAYMENT_METHOD"].ToString(),
                InvoiceItems = GetOrderDetails(Convert.ToInt32(row["ORDER_ID"])),
                //CompleteTime = row["COMPLETED_TIME"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["COMPLETED_TIME"]),
            });
        }

        return orders;
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
        var query = "SELECT BEVERAGE_ID FROM BEVERAGE_SIZE WHERE BEVERAGE_ID = @beverageId AND SIZE = @size";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@beverageId", beverageId),
            new MySqlParameter("@size", size)
        };

        var result = ExecuteSelectQuery(query, parameters);

        return result.Count > 0 ? Convert.ToInt32(result[0]["BEVERAGE_ID"]) : -1;
    }

    public async Task AddOrderDetail(int orderId, InvoiceItem item)
    {
        var query = "INSERT INTO ORDER_DETAILS (ORDER_ID, BEVERAGE_SIZE_ID, QUANTITY, PRICE, SUBTOTAL) VALUES (@orderId, @beverageSizeId, @quantity, @price, @total)";
        var parameters = new List<MySqlParameter>
        {
            new ("@orderId", orderId),
            new ("@beverageSizeId", MySqlDbType.Int32) { Value = GetBeverageSizeId(item.BeverageId, item.Size) },
            new ("@quantity", item.Quantity),
            new ("@price", item.Price),
            new ("@total", item.Total)

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
                MaterialCode = row["ID"].ToString(),
                MaterialName = row["MATERIAL_NAME"].ToString(),
                Quantity = Convert.ToInt32(row["QUANTITY"]),
                UnitPrice = Convert.ToInt32(row["PRICE"])
            });
        }

        return materials;
    }

    public Material GetMaterialByCode(string id)
    {
        var query = "SELECT * FROM MATERIAL WHERE ID = @id";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@id", id)
        };

        var result = ExecuteSelectQuery(query, parameters);

        if (result.Count == 0) return null;

        var row = result[0];
        return new Material
        {
            MaterialCode = row["ID"].ToString(),
            MaterialName = row["MATERIAL_NAME"].ToString(),
            Quantity = Convert.ToInt32(row["QUANTITY"]),
            UnitPrice = Convert.ToInt32(row["PRICE"])
        };
    }

    public bool AddMaterial(Material material)
    {
        var query = "INSERT INTO MATERIAL (ID, MATERIAL_NAME, QUANTITY, PRICE) VALUES (@id, @name, @quantity, @price)";
        var parameters = new List<MySqlParameter>
        {
            new ("@id", material.MaterialCode),
            new ("@name", material.MaterialName),
            new ("@quantity", material.Quantity),
            new ("@price", material.UnitPrice)
        };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public bool UpdateMaterial(Material material)
    {
        var query = "UPDATE MATERIAL SET MATERIAL_NAME = @name, QUANTITY = @quantity, PRICE = @price WHERE ID = @id";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@id", material.MaterialCode),
            new MySqlParameter("@name", material.MaterialName),
            new MySqlParameter("@quantity", material.Quantity),
            new MySqlParameter("@price", material.UnitPrice)
        };

        return ExecuteNonQuery(query, parameters) > 0;
    }

    public bool DeleteMaterial(string id)
    {
        var query = "DELETE FROM MATERIAL WHERE ID = @id";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@id", id)
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
        var query = "SELECT * FROM ACCOUNT WHERE USERNAME = @username";
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@username", username)
        };

        var result = ExecuteSelectQuery(query, parameters);

        if (result.Count == 0) return null;

        var row = result[0];
        return new User
        {
            Username = row["USERNAME"].ToString(),
            Password = row["USER_PASSWORD"].ToString()
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
            t.CATEGORY
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

    public User GetCurrentUser(string username)
    {
        var query = "SELECT * FROM USERS WHERE USERNAME = @username"; // Modify according to your database schema
        var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@username", username)
        };

        var result = ExecuteSelectQuery(query, parameters);

        var row = result[0];

        return new User
        {
            Username = row["USERNAME"].ToString(),
            Password = row["USER_PASSWORD"].ToString(),
            AccessLevel = Convert.ToInt32(row["AccessLevel"])
        };
    }
}

