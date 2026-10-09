using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<int> RecordPaymentWithAllocationsAsync(Payment payment, IEnumerable<PaymentAllocation> allocations, IEnumerable<Invoice> updatedInvoices);
    }
}
