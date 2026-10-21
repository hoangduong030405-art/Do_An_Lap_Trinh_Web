using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Tests.Fakes
{
    public class FakePaymentRepository : IPaymentRepository
    {
        public List<Payment> Payments { get; } = new();
        public List<PaymentAllocation> Allocations { get; } = new();

        public Task<IEnumerable<Payment>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Payment>>(Payments);
        }

        public Task<int> RecordPaymentWithAllocationsAsync(
            Payment payment, 
            IEnumerable<PaymentAllocation> allocations, 
            IEnumerable<Invoice> updatedInvoices)
        {
            payment.Id = Payments.Any() ? Payments.Max(p => p.Id) + 1 : 1;
            Payments.Add(payment);

            foreach (var alloc in allocations)
            {
                alloc.PaymentId = payment.Id;
                alloc.Id = Allocations.Any() ? Allocations.Max(a => a.Id) + 1 : 1;
                Allocations.Add(alloc);
            }

            return Task.FromResult(payment.Id);
        }

        public Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryWithDetailsAsync()
        {
            return Task.FromResult<IEnumerable<PaymentHistoryDto>>(new List<PaymentHistoryDto>());
        }

        public Task<PaymentHistoryDto?> GetPaymentDetailByIdAsync(int paymentId)
        {
            return Task.FromResult<PaymentHistoryDto?>(null);
        }

        public Task<CustomerLedgerDto> GetCustomerLedgerAsync(int customerId)
        {
            return Task.FromResult(new CustomerLedgerDto());
        }
    }
}
