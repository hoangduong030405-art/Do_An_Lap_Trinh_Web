using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<NotificationItemDto>> GetNotificationsAsync(string role, int? customerId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var notifications = new List<NotificationItemDto>();

            // 1. Invoices Overdue or Due within 3 days
            string invoiceQuery;
            if (role == "Customer" && customerId.HasValue)
            {
                invoiceQuery = @"
                    SELECT TOP 8
                        i.Id,
                        CASE 
                            WHEN i.DueDate < GETDATE() THEN CONCAT('Hóa đơn #', i.InvoiceNumber, ' đã quá hạn!')
                            ELSE CONCAT('Hóa đơn #', i.InvoiceNumber, ' sắp đến hạn thanh toán')
                        END AS Title,
                        CONCAT('Số dư nợ còn lại: ', FORMAT(i.Balance, 'N0'), ' đ (Hạn chót: ', FORMAT(i.DueDate, 'dd/MM/yyyy'), ')') AS Message,
                        CASE WHEN i.DueDate < GETDATE() THEN 'danger' ELSE 'warning' END AS Type,
                        '/portal/invoices' AS Url,
                        i.DueDate AS CreatedAt,
                        CASE WHEN i.DueDate < GETDATE() THEN 'bi-exclamation-octagon-fill' ELSE 'bi-hourglass-split' END AS Icon
                    FROM Invoices i
                    WHERE i.CustomerId = @CustomerId AND i.Balance > 0 AND i.DueDate <= DATEADD(DAY, 3, GETDATE())
                    ORDER BY i.DueDate ASC;
                ";
            }
            else
            {
                invoiceQuery = @"
                    SELECT TOP 8
                        i.Id,
                        CASE 
                            WHEN i.DueDate < GETDATE() THEN CONCAT('Nợ quá hạn: HĐ #', i.InvoiceNumber, ' - ', c.FullName)
                            ELSE CONCAT('HĐ #', i.InvoiceNumber, ' sắp đến hạn - ', c.FullName)
                        END AS Title,
                        CONCAT('Còn nợ: ', FORMAT(i.Balance, 'N0'), ' đ / Tổng ', FORMAT(i.TotalAmount, 'N0'), ' đ (Hạn: ', FORMAT(i.DueDate, 'dd/MM/yyyy'), ')') AS Message,
                        CASE WHEN i.DueDate < GETDATE() THEN 'danger' ELSE 'warning' END AS Type,
                        '/invoices' AS Url,
                        i.DueDate AS CreatedAt,
                        CASE WHEN i.DueDate < GETDATE() THEN 'bi-exclamation-octagon-fill' ELSE 'bi-hourglass-split' END AS Icon
                    FROM Invoices i
                    INNER JOIN Customers c ON i.CustomerId = c.Id
                    WHERE i.Balance > 0 AND i.DueDate <= DATEADD(DAY, 3, GETDATE())
                    ORDER BY i.DueDate ASC;
                ";
            }

            var invoices = await connection.QueryAsync<NotificationItemDto>(invoiceQuery, new
            {
                CustomerId = customerId ?? 0
            });
            notifications.AddRange(invoices);

            // 2. Customers Near / Exceeding Credit Limit (Accountant & Sales only)
            if (role != "Customer")
            {
                const string creditLimitQuery = @"
                    WITH CustomerDebt AS (
                        SELECT CustomerId, SUM(Balance) AS TotalDebt
                        FROM Invoices
                        WHERE Balance > 0
                        GROUP BY CustomerId
                    )
                    SELECT TOP 5
                        c.Id,
                        CONCAT('Cảnh báo hạn mức: ', c.FullName) AS Title,
                        CONCAT('Đang nợ ', FORMAT(cd.TotalDebt, 'N0'), ' đ trên trần hạn mức ', FORMAT(c.CreditLimit, 'N0'), ' đ (', FORMAT(cd.TotalDebt / NULLIF(c.CreditLimit, 0) * 100, 'N1'), '%)') AS Message,
                        'warning' AS Type,
                        '/manage-customers' AS Url,
                        GETDATE() AS CreatedAt,
                        'bi-shield-exclamation' AS Icon
                    FROM Customers c
                    INNER JOIN CustomerDebt cd ON c.Id = cd.CustomerId
                    WHERE c.CreditLimit > 0 AND cd.TotalDebt >= (c.CreditLimit * 0.8)
                    ORDER BY (cd.TotalDebt / c.CreditLimit) DESC;
                ";

                var creditWarnings = await connection.QueryAsync<NotificationItemDto>(creditLimitQuery);
                notifications.AddRange(creditWarnings);

                // 3. Low Stock Items (Accountant & Sales only)
                const string stockQuery = @"
                    SELECT TOP 5
                        Id,
                        CONCAT('Hàng sắp hết: ', Name) AS Title,
                        CONCAT('Tồn kho chỉ còn ', StockQuantity, ' ', Unit, ' (Dưới ngưỡng an toàn 15)') AS Message,
                        'info' AS Type,
                        '/manage-products' AS Url,
                        GETDATE() AS CreatedAt,
                        'bi-box-seam' AS Icon
                    FROM Products
                    WHERE StockQuantity <= 15
                    ORDER BY StockQuantity ASC;
                ";

                var stockWarnings = await connection.QueryAsync<NotificationItemDto>(stockQuery);
                notifications.AddRange(stockWarnings);
            }

            return notifications;
        }
    }
}
