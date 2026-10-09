using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
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

        public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryWithDetailsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sqlHeader = @"
                SELECT p.Id, p.PaymentNumber, p.CustomerId, c.CustomerCode, c.FullName AS CustomerName,
                       c.PhoneNumber AS CustomerPhone, c.Address AS CustomerAddress,
                       p.PaymentDate, p.Amount, p.UnallocatedAmount, p.Note
                FROM Payments p
                INNER JOIN Customers c ON p.CustomerId = c.Id
                ORDER BY p.PaymentDate DESC";

            var payments = (await connection.QueryAsync<PaymentHistoryDto>(sqlHeader)).ToList();

            var sqlAllocations = @"
                SELECT pa.PaymentId, pa.InvoiceId, i.InvoiceNumber, i.InvoiceDate, i.TotalAmount AS InvoiceTotal, pa.AllocatedAmount
                FROM PaymentAllocations pa
                INNER JOIN Invoices i ON pa.InvoiceId = i.Id";

            var allocations = await connection.QueryAsync<(int PaymentId, int InvoiceId, string InvoiceNumber, System.DateTime InvoiceDate, decimal InvoiceTotal, decimal AllocatedAmount)>(sqlAllocations);

            var allocGroup = allocations.GroupBy(a => a.PaymentId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var p in payments)
            {
                if (allocGroup.TryGetValue(p.Id, out var allocList))
                {
                    p.Allocations = allocList.Select(a => new PaymentAllocationDetailDto
                    {
                        InvoiceId = a.InvoiceId,
                        InvoiceNumber = a.InvoiceNumber,
                        InvoiceDate = a.InvoiceDate,
                        InvoiceTotal = a.InvoiceTotal,
                        AllocatedAmount = a.AllocatedAmount
                    }).ToList();
                }
            }

            return payments;
        }

        public async Task<PaymentHistoryDto?> GetPaymentDetailByIdAsync(int paymentId)
        {
            var history = await GetPaymentHistoryWithDetailsAsync();
            return history.FirstOrDefault(p => p.Id == paymentId);
        }

        public async Task<CustomerLedgerDto> GetCustomerLedgerAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var customer = await connection.QuerySingleOrDefaultAsync<Customer>("SELECT * FROM Customers WHERE Id = @Id", new { Id = customerId });

            var ledger = new CustomerLedgerDto
            {
                CustomerId = customerId,
                CustomerCode = customer?.CustomerCode ?? "",
                CustomerName = customer?.FullName ?? "",
                CustomerPhone = customer?.PhoneNumber ?? "",
                CustomerAddress = customer?.Address ?? "",
                CreditLimit = customer?.CreditLimit ?? 0
            };

            var invoices = await connection.QueryAsync<Invoice>("SELECT * FROM Invoices WHERE CustomerId = @CustomerId ORDER BY InvoiceDate ASC", new { CustomerId = customerId });
            var payments = await connection.QueryAsync<Payment>("SELECT * FROM Payments WHERE CustomerId = @CustomerId ORDER BY PaymentDate ASC", new { CustomerId = customerId });

            var list = new List<LedgerEntryDto>();

            foreach (var inv in invoices)
            {
                list.Add(new LedgerEntryDto
                {
                    TransactionDate = inv.InvoiceDate,
                    DocumentType = "Hóa Đơn",
                    DocumentNumber = inv.InvoiceNumber,
                    Description = $"Xuất bán hóa đơn #{inv.InvoiceNumber}",
                    Debit = inv.TotalAmount,
                    Credit = 0
                });
            }

            foreach (var p in payments)
            {
                list.Add(new LedgerEntryDto
                {
                    TransactionDate = p.PaymentDate,
                    DocumentType = "Phiếu Thu",
                    DocumentNumber = p.PaymentNumber,
                    Description = string.IsNullOrEmpty(p.Note) ? $"Thu tiền phân bổ FIFO #{p.PaymentNumber}" : p.Note,
                    Debit = 0,
                    Credit = p.Amount
                });
            }

            // Sắp xếp theo ngày phát sinh
            var ordered = list.OrderBy(x => x.TransactionDate).ToList();

            decimal running = 0;
            foreach (var entry in ordered)
            {
                running += (entry.Debit - entry.Credit);
                entry.RunningBalance = running;
            }

            ledger.Entries = ordered;
            ledger.TotalDebit = ordered.Sum(x => x.Debit);
            ledger.TotalCredit = ordered.Sum(x => x.Credit);

            return ledger;
        }
    }
}
