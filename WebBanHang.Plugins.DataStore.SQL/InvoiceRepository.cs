using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
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
    }
}
