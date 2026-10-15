using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class GlobalSearchRepository : IGlobalSearchRepository
    {
        private readonly string _connectionString;

        public GlobalSearchRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<GlobalSearchResultDto>> SearchAsync(string keyword, string role, int? customerId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var results = new List<GlobalSearchResultDto>();
            string searchParam = $"%{keyword.Trim()}%";

            // 1. SEARCH PRODUCTS (Available to all roles)
            const string productQuery = @"
                SELECT TOP 5 
                    Id, 
                    Name AS Title, 
                    CONCAT(ProductCode, ' • ĐVT: ', Unit, ' • Giá: ', FORMAT(UnitPrice, 'N0'), ' đ') AS Subtitle,
                    'Sản phẩm' AS Category,
                    CASE WHEN @Role = 'Customer' THEN '/portal/store' ELSE '/manage-products' END AS Url,
                    CONCAT(StockQuantity, ' ', Unit) AS BadgeText,
                    'info' AS BadgeColor
                FROM Products
                WHERE Name LIKE @Keyword OR ProductCode LIKE @Keyword
                ORDER BY Name ASC;
            ";

            var products = await connection.QueryAsync<GlobalSearchResultDto>(productQuery, new
            {
                Keyword = searchParam,
                Role = role
            });
            results.AddRange(products);

            // 2. SEARCH INVOICES (Filtered by customer if role == Customer)
            string invoiceQuery;
            if (role == "Customer" && customerId.HasValue)
            {
                invoiceQuery = @"
                    SELECT TOP 5 
                        i.Id, 
                        CONCAT('Hóa đơn #', i.InvoiceNumber) AS Title, 
                        CONCAT('Tổng tiền: ', FORMAT(i.TotalAmount, 'N0'), ' đ • Còn nợ: ', FORMAT(i.Balance, 'N0'), ' đ') AS Subtitle,
                        'Hóa đơn' AS Category,
                        '/portal/invoices' AS Url,
                        CASE WHEN i.Balance <= 0 THEN 'Đã trả' ELSE 'Còn nợ' END AS BadgeText,
                        CASE WHEN i.Balance <= 0 THEN 'success' ELSE 'warning' END AS BadgeColor
                    FROM Invoices i
                    WHERE i.CustomerId = @CustomerId AND i.InvoiceNumber LIKE @Keyword
                    ORDER BY i.InvoiceDate DESC;
                ";
            }
            else
            {
                invoiceQuery = @"
                    SELECT TOP 5 
                        i.Id, 
                        CONCAT('Hóa đơn #', i.InvoiceNumber) AS Title, 
                        CONCAT(c.FullName, ' • Nợ: ', FORMAT(i.Balance, 'N0'), ' đ / ', FORMAT(i.TotalAmount, 'N0'), ' đ') AS Subtitle,
                        'Hóa đơn' AS Category,
                        '/invoices' AS Url,
                        CASE WHEN i.Balance <= 0 THEN 'Đã trả' ELSE 'Còn nợ' END AS BadgeText,
                        CASE WHEN i.Balance <= 0 THEN 'success' ELSE 'warning' END AS BadgeColor
                    FROM Invoices i
                    INNER JOIN Customers c ON i.CustomerId = c.Id
                    WHERE i.InvoiceNumber LIKE @Keyword OR c.FullName LIKE @Keyword OR c.CustomerCode LIKE @Keyword
                    ORDER BY i.InvoiceDate DESC;
                ";
            }

            var invoices = await connection.QueryAsync<GlobalSearchResultDto>(invoiceQuery, new
            {
                Keyword = searchParam,
                CustomerId = customerId ?? 0
            });
            results.AddRange(invoices);

            // 3. SEARCH CUSTOMERS (Only for Sales & Accountant)
            if (role != "Customer")
            {
                const string customerQuery = @"
                    SELECT TOP 5 
                        Id, 
                        FullName AS Title, 
                        CONCAT('Mã: ', CustomerCode, ' • SĐT: ', PhoneNumber, ' • Hạn mức: ', FORMAT(CreditLimit, 'N0'), ' đ') AS Subtitle,
                        'Khách hàng' AS Category,
                        '/manage-customers' AS Url,
                        CustomerCode AS BadgeText,
                        'primary' AS BadgeColor
                    FROM Customers
                    WHERE FullName LIKE @Keyword OR CustomerCode LIKE @Keyword OR PhoneNumber LIKE @Keyword
                    ORDER BY FullName ASC;
                ";

                var customers = await connection.QueryAsync<GlobalSearchResultDto>(customerQuery, new
                {
                    Keyword = searchParam
                });
                results.AddRange(customers);
            }

            return results;
        }
    }
}
