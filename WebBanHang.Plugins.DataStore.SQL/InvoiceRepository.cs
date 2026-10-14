using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Invoice>("SELECT * FROM Invoices ORDER BY InvoiceDate DESC");
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Invoice>("SELECT * FROM Invoices WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesByCustomerOrderedByDateAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId AND Balance > 0 AND Status != @Status ORDER BY InvoiceDate ASC";
            return await connection.QueryAsync<Invoice>(query, new { CustomerId = customerId, Status = (int)InvoiceStatus.Cancelled });
        }

        public async Task<int> CreateInvoiceWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
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
                    Status = (int)invoice.Status
                }, transaction);

                var insertLineQuery = @"
                    INSERT INTO InvoiceLines (InvoiceId, ProductId, Quantity, UnitPrice)
                    VALUES (@InvoiceId, @ProductId, @Quantity, @UnitPrice)";

                var updateProductQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId";

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

                    await connection.ExecuteAsync(updateProductQuery, new
                    {
                        line.Quantity,
                        line.ProductId
                    }, transaction);
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
                ORDER BY i.InvoiceDate DESC";

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
                SELECT l.ProductId, p.ProductCode, p.Name AS ProductName, p.Unit, l.Quantity, l.UnitPrice
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
                SELECT 
                    MONTH(i.InvoiceDate) AS MonthNumber,
                    FORMAT(i.InvoiceDate, 'MM/yyyy') AS MonthLabel,
                    SUM(i.TotalAmount) AS Revenue,
                    SUM(i.PaidAmount) AS Collected,
                    SUM(i.TotalAmount * 0.72) AS Cost
                FROM Invoices i
                WHERE i.InvoiceDate >= DATEADD(month, -@Months, GETDATE())
                GROUP BY FORMAT(i.InvoiceDate, 'MM/yyyy'), YEAR(i.InvoiceDate), MONTH(i.InvoiceDate)
                ORDER BY YEAR(i.InvoiceDate) ASC, MONTH(i.InvoiceDate) ASC";

            return await connection.QueryAsync<MonthlyRevenueDto>(sql, new { Months = months });
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId ORDER BY InvoiceDate DESC";
            return await connection.QueryAsync<Invoice>(query, new { CustomerId = customerId });
        }
    }
}
