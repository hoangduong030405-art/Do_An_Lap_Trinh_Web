using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Invoices
{
    public interface IGetCustomerMyInvoicesUseCase
    {
        Task<IEnumerable<CustomerPortalInvoiceDto>> ExecuteAsync(int customerId, string? filterStatus = null);
    }
}
