using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Payment>("SELECT * FROM Payments ORDER BY PaymentDate DESC");
        }

        public async Task<int> RecordPaymentWithAllocationsAsync(Payment payment, IEnumerable<PaymentAllocation> allocations, IEnumerable<Invoice> updatedInvoices)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                var insertPaymentQuery = @"
                    INSERT INTO Payments (PaymentNumber, CustomerId, PaymentDate, Amount, UnallocatedAmount, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@PaymentNumber, @CustomerId, @PaymentDate, @Amount, @UnallocatedAmount, @Note)";

                var paymentId = await connection.QuerySingleAsync<int>(insertPaymentQuery, new
                {
                    payment.PaymentNumber,
                    payment.CustomerId,
                    payment.PaymentDate,
                    payment.Amount,
                    payment.UnallocatedAmount,
                    payment.Note
                }, transaction);

                var insertAllocationQuery = @"
                    INSERT INTO PaymentAllocations (PaymentId, InvoiceId, AllocatedAmount, AllocationDate)
                    VALUES (@PaymentId, @InvoiceId, @AllocatedAmount, @AllocationDate)";

                foreach (var allocation in allocations)
                {
                    allocation.PaymentId = paymentId;
                    await connection.ExecuteAsync(insertAllocationQuery, new
                    {
                        allocation.PaymentId,
                        allocation.InvoiceId,
                        allocation.AllocatedAmount,
                        allocation.AllocationDate
                    }, transaction);
                }

                var updateInvoiceQuery = "UPDATE Invoices SET PaidAmount = @PaidAmount, Balance = @Balance, Status = @Status WHERE Id = @Id";

                foreach (var invoice in updatedInvoices)
                {
                    await connection.ExecuteAsync(updateInvoiceQuery, new
                    {
                        invoice.PaidAmount,
                        invoice.Balance,
                        Status = (int)invoice.Status,
                        invoice.Id
                    }, transaction);
                }

                transaction.Commit();
                return paymentId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
