using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<int> RecordPaymentWithAllocationsAsync(Payment payment, IEnumerable<PaymentAllocation> allocations, IEnumerable<Invoice> updatedInvoices);
        Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryWithDetailsAsync();
        Task<PaymentHistoryDto?> GetPaymentDetailByIdAsync(int paymentId);
        Task<CustomerLedgerDto> GetCustomerLedgerAsync(int customerId);
    }
}
