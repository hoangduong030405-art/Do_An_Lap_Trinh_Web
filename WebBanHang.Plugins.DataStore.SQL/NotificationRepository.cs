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
            var asOfDate = DateTime.Now;

            // 1. BIẾN ĐỘNG VIETQR WEBHOOK (Dữ liệu thanh toán thực tế)
            string paymentQuery;
            if (role == "Customer" && customerId.HasValue)
            {
                paymentQuery = @"
                    SELECT TOP 3
                        p.Id,
                        N'Thanh toán VietQR thành công' AS Title,
                        CONCAT(N'Đã nhận chuyển khoản +', FORMAT(p.Amount, 'N0'), N' đ qua ACB — Tự động gạch nợ FIFO vào hóa đơn cũ nhất.') AS Message,
                        'success' AS Type,
                        '/portal/invoices' AS Url,
                        p.PaymentDate AS CreatedAt,
                        'bi-lightning-charge-fill' AS Icon
                    FROM Payments p
                    WHERE p.CustomerId = @CustomerId
                    ORDER BY p.PaymentDate DESC, p.Id DESC;
                ";
            }
            else
            {
                paymentQuery = @"
                    SELECT TOP 3
                        p.Id,
                        N'Biến động VietQR Webhook (ACB)' AS Title,
                        CONCAT(N'Ngân hàng ACB báo Có +', FORMAT(p.Amount, 'N0'), N' đ từ ', c.FullName, N' — Đã tự động gạch nợ FIFO vào HĐ cũ nhất.') AS Message,
                        'success' AS Type,
                        '/payment-history' AS Url,
                        p.PaymentDate AS CreatedAt,
                        'bi-lightning-charge-fill' AS Icon
                    FROM Payments p
                    INNER JOIN Customers c ON p.CustomerId = c.Id
                    ORDER BY p.PaymentDate DESC, p.Id DESC;
                ";
            }

            var payments = await connection.QueryAsync<NotificationItemDto>(paymentQuery, new
            {
                CustomerId = customerId ?? 0
            });
            notifications.AddRange(payments);

            // 2. CẢNH BÁO NỢ QUÁ HẠN (>90 ngày & 31-60 ngày)
            string invoiceQuery;
            if (role == "Customer" && customerId.HasValue)
            {
                invoiceQuery = @"
                    SELECT TOP 5
                        i.Id,
                        CASE 
                            WHEN CAST(i.DueDate AS DATE) < CAST(@AsOfDate AS DATE) THEN CONCAT(N'Hóa đơn #', i.InvoiceNumber, N' đã quá hạn!')
                            ELSE CONCAT(N'Hóa đơn #', i.InvoiceNumber, N' sắp đến hạn')
                        END AS Title,
                        CONCAT(N'Số dư nợ còn lại: ', FORMAT(i.Balance, 'N0'), N' đ (Hạn chót: ', FORMAT(i.DueDate, 'dd/MM/yyyy'), N')') AS Message,
                        CASE WHEN CAST(i.DueDate AS DATE) < CAST(@AsOfDate AS DATE) THEN 'danger' ELSE 'warning' END AS Type,
                        '/portal/invoices' AS Url,
                        i.DueDate AS CreatedAt,
                        CASE WHEN CAST(i.DueDate AS DATE) < CAST(@AsOfDate AS DATE) THEN 'bi-exclamation-octagon-fill' ELSE 'bi-hourglass-split' END AS Icon
                    FROM Invoices i
                    WHERE i.CustomerId = @CustomerId 
                      AND i.Balance > 0 
                      AND CAST(i.DueDate AS DATE) <= DATEADD(DAY, 7, CAST(@AsOfDate AS DATE))
                    ORDER BY i.DueDate ASC;
                ";
            }
            else
            {
                invoiceQuery = @"
                    SELECT TOP 5
                        i.Id,
                        CASE 
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) > 90 THEN CONCAT(N'Cảnh báo Nợ xấu (>90 ngày): ', c.FullName)
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) >= 31 THEN CONCAT(N'Cảnh báo Quá hạn 31-60 ngày: ', c.FullName)
                            ELSE CONCAT(N'HĐ #', i.InvoiceNumber, N' sắp đến hạn - ', c.FullName)
                        END AS Title,
                        CASE 
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) > 90 THEN CONCAT(c.FullName, N' đang có ', FORMAT(i.Balance, 'N0'), N' đ nợ quá hạn trên 90 ngày (Hạng C - Cảnh báo).')
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) >= 31 THEN CONCAT(c.FullName, N' có hóa đơn #', i.InvoiceNumber, N' quá hạn ', DATEDIFF(DAY, i.DueDate, @AsOfDate), N' ngày (Tổng ', FORMAT(i.Balance, 'N0'), N' đ) cần đôn đốc.')
                            ELSE CONCAT(N'Còn nợ: ', FORMAT(i.Balance, 'N0'), N' đ / Tổng ', FORMAT(i.TotalAmount, 'N0'), N' đ (Hạn: ', FORMAT(i.DueDate, 'dd/MM/yyyy'), N')')
                        END AS Message,
                        CASE 
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) > 90 THEN 'danger' 
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) >= 31 THEN 'warning'
                            ELSE 'info'
                        END AS Type,
                        '/aging-report' AS Url,
                        i.DueDate AS CreatedAt,
                        CASE 
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) > 90 THEN 'bi-exclamation-octagon-fill'
                            WHEN DATEDIFF(DAY, i.DueDate, @AsOfDate) >= 31 THEN 'bi-exclamation-triangle-fill'
                            ELSE 'bi-clock-history'
                        END AS Icon
                    FROM Invoices i
                    INNER JOIN Customers c ON i.CustomerId = c.Id
                    WHERE i.Balance > 0 
                      AND CAST(i.DueDate AS DATE) <= DATEADD(DAY, 3, CAST(@AsOfDate AS DATE))
                    ORDER BY i.DueDate ASC;
                ";
            }

            var invoices = await connection.QueryAsync<NotificationItemDto>(invoiceQuery, new
            {
                CustomerId = customerId ?? 0,
                AsOfDate = asOfDate
            });
            notifications.AddRange(invoices);

            // 3. TRẠNG THÁI HẠN MỨC TÍN DỤNG & TỒN KHO (Sales & Accountant)
            if (role != "Customer")
            {
                const string creditLimitQuery = @"
                    WITH CustomerDebt AS (
                        SELECT CustomerId, SUM(Balance) AS TotalDebt
                        FROM Invoices
                        WHERE Balance > 0
                        GROUP BY CustomerId
                    )
                    SELECT TOP 3
                        c.Id,
                        CONCAT(N'Trạng thái Hạn mức: ', c.FullName) AS Title,
                        CONCAT(c.FullName, N' đang sử dụng ', FORMAT(ISNULL(cd.TotalDebt, 0) / NULLIF(c.CreditLimit, 0) * 100, 'N1'), N'% hạn mức tín dụng (Khả dụng ', FORMAT(c.CreditLimit - ISNULL(cd.TotalDebt, 0), 'N0'), N' đ).') AS Message,
                        'info' AS Type,
                        '/manage-customers' AS Url,
                        GETDATE() AS CreatedAt,
                        'bi-shield-check' AS Icon
                    FROM Customers c
                    LEFT JOIN CustomerDebt cd ON c.Id = cd.CustomerId
                    WHERE c.CreditLimit > 0
                    ORDER BY (ISNULL(cd.TotalDebt, 0) / c.CreditLimit) DESC;
                ";

                var creditWarnings = await connection.QueryAsync<NotificationItemDto>(creditLimitQuery);
                notifications.AddRange(creditWarnings);
            }

            return notifications;
        }
    }
}
