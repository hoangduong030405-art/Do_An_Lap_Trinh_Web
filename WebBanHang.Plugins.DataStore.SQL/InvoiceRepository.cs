using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        static InvoiceRepository()
        {
            SqlMapper.AddTypeHandler(new InvoiceStatusTypeHandler());
        }

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Invoice>("SELECT * FROM Invoices ORDER BY InvoiceDate DESC, Id DESC");
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Invoice>("SELECT * FROM Invoices WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesByCustomerOrderedByDateAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId AND Balance > 0 AND Status NOT IN ('Cancelled', '4', 'Paid', '3') ORDER BY InvoiceDate ASC, Id ASC";
            return await connection.QueryAsync<Invoice>(query, new { CustomerId = customerId });
        }

        public async Task<int> CreateInvoiceWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Khóa dòng khách hàng với (UPDLOCK, HOLDLOCK) và kiểm tra lại hạn mức tín dụng trong cùng transaction (chống tranh chấp đồng thời)
                var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
                    "SELECT Id, CustomerCode, FullName, CreditLimit, PaymentTermsDays FROM Customers WITH (UPDLOCK, HOLDLOCK) WHERE Id = @CustomerId",
                    new { invoice.CustomerId },
                    transaction);

                if (customer == null)
                {
                    throw new BusinessRuleException("Không tìm thấy thông tin khách hàng.");
                }

                var currentDebt = await connection.ExecuteScalarAsync<decimal>(
                    "SELECT ISNULL(SUM(Balance), 0) FROM Invoices WHERE CustomerId = @CustomerId AND Balance > 0 AND Status NOT IN ('Cancelled', '4')",
                    new { invoice.CustomerId },
                    transaction);

                if (currentDebt + invoice.TotalAmount > customer.CreditLimit)
                {
                    throw new CreditLimitExceededException(
                        $"Không thể lập hóa đơn! Khách hàng '{customer.FullName}' đang nợ {currentDebt:N0} đ. Đơn mới {invoice.TotalAmount:N0} đ khiến tổng nợ ({currentDebt + invoice.TotalAmount:N0} đ) vượt quá hạn mức tín dụng cho phép ({customer.CreditLimit:N0} đ).");
                }

                var insertInvoiceQuery = @"
                    INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, DueDate, TotalAmount, PaidAmount, Balance, Status)
                    OUTPUT INSERTED.Id
                    VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @DueDate, @TotalAmount, @PaidAmount, @Balance, @Status)";
                
                var invoiceId = await connection.QuerySingleAsync<int>(insertInvoiceQuery, new
                {
                    invoice.InvoiceNumber,
                    invoice.CustomerId,
                    invoice.InvoiceDate,
                    invoice.DueDate,
                    invoice.TotalAmount,
                    invoice.PaidAmount,
                    invoice.Balance,
                    Status = invoice.Status.ToString()
                }, transaction);

                var insertLineQuery = @"
                    INSERT INTO InvoiceLines (InvoiceId, ProductId, Quantity, UnitPrice, CostPrice)
                    VALUES (@InvoiceId, @ProductId, @Quantity, @UnitPrice, (SELECT COALESCE(CostPrice, 0) FROM Products WHERE Id = @ProductId))";

                var updateProductQuery = @"
                    UPDATE Products 
                    SET StockQuantity = StockQuantity - @Quantity 
                    WHERE Id = @ProductId AND StockQuantity >= @Quantity";

                foreach (var line in lines)
                {
                    line.InvoiceId = invoiceId;
                    await connection.ExecuteAsync(insertLineQuery, new
                    {
                        InvoiceId = invoiceId,
                        line.ProductId,
                        line.Quantity,
                        line.UnitPrice
                    }, transaction);

                    var rowsAffected = await connection.ExecuteAsync(updateProductQuery, new
                    {
                        line.Quantity,
                        line.ProductId
                    }, transaction);

                    if (rowsAffected == 0)
                    {
                        throw new BusinessRuleException($"Sản phẩm mã #{line.ProductId} không đủ tồn kho.");
                    }
                }

                transaction.Commit();
                return invoiceId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<InvoiceListDto>> GetInvoicesWithCustomerAsync(string? keyword = null, string? status = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT i.Id, i.InvoiceNumber, i.CustomerId, c.CustomerCode, c.FullName AS CustomerName,
                       i.InvoiceDate, i.DueDate, i.TotalAmount, i.PaidAmount, i.Balance, i.Status
                FROM Invoices i
                INNER JOIN Customers c ON i.CustomerId = c.Id
                WHERE (@Keyword IS NULL OR i.InvoiceNumber LIKE @KeywordSearch OR c.FullName LIKE @KeywordSearch OR c.CustomerCode LIKE @KeywordSearch)
                  AND (@Status IS NULL OR i.Status = @Status)
                ORDER BY i.InvoiceDate DESC, i.Id DESC";

            return await connection.QueryAsync<InvoiceListDto>(sql, new
            {
                Keyword = keyword,
                KeywordSearch = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword}%",
                Status = status
            });
        }

        public async Task<InvoiceDetailDto?> GetInvoiceDetailByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var headerSql = @"
                SELECT i.Id, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.TotalAmount, i.PaidAmount, i.Balance, i.Status,
                       i.CustomerId, c.CustomerCode, c.FullName AS CustomerName, c.PhoneNumber AS CustomerPhone, c.Address AS CustomerAddress
                FROM Invoices i
                INNER JOIN Customers c ON i.CustomerId = c.Id
                WHERE i.Id = @Id";

            var dto = await connection.QuerySingleOrDefaultAsync<InvoiceDetailDto>(headerSql, new { Id = id });
            if (dto == null) return null;

            var linesSql = @"
                SELECT l.ProductId, p.ProductCode, p.Name AS ProductName, p.Unit, l.Quantity, l.UnitPrice, COALESCE(l.CostPrice, p.CostPrice, 0) AS CostPrice
                FROM InvoiceLines l
                INNER JOIN Products p ON l.ProductId = p.Id
                WHERE l.InvoiceId = @Id";

            var lines = await connection.QueryAsync<InvoiceLineDetailDto>(linesSql, new { Id = id });
            dto.Lines = lines.ToList();

            return dto;
        }

        public async Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int top = 5)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT TOP (@Top) l.ProductId, p.ProductCode, p.Name AS ProductName, p.ImageUrl,
                       SUM(l.Quantity) AS TotalQuantitySold, SUM(l.Quantity * l.UnitPrice) AS TotalRevenue
                FROM InvoiceLines l
                INNER JOIN Products p ON l.ProductId = p.Id
                GROUP BY l.ProductId, p.ProductCode, p.Name, p.ImageUrl
                ORDER BY TotalQuantitySold DESC";

            return await connection.QueryAsync<TopSellingProductDto>(sql, new { Top = top });
        }

        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueStatisticsAsync(int months = 12)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                WITH InvoiceCost AS (
                    SELECT InvoiceId, SUM(Quantity * CostPrice) AS TotalCost
                    FROM InvoiceLines
                    GROUP BY InvoiceId
                )
                SELECT 
                    MONTH(i.InvoiceDate) AS MonthNumber,
                    FORMAT(i.InvoiceDate, 'MM/yyyy') AS MonthLabel,
                    SUM(i.TotalAmount) AS Revenue,
                    SUM(i.PaidAmount) AS Collected,
                    SUM(COALESCE(c.TotalCost, 0)) AS Cost
                FROM Invoices i
                LEFT JOIN InvoiceCost c ON i.Id = c.InvoiceId
                WHERE i.InvoiceDate >= DATEADD(month, -@Months, GETDATE())
                GROUP BY FORMAT(i.InvoiceDate, 'MM/yyyy'), YEAR(i.InvoiceDate), MONTH(i.InvoiceDate)
                ORDER BY YEAR(i.InvoiceDate) ASC, MONTH(i.InvoiceDate) ASC";

            return await connection.QueryAsync<MonthlyRevenueDto>(sql, new { Months = months });
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId ORDER BY InvoiceDate DESC, Id DESC";
            return await connection.QueryAsync<Invoice>(query, new { CustomerId = customerId });
        }
    }
}
