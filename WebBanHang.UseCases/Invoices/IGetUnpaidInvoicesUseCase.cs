using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.Invoices
{
    public interface IGetUnpaidInvoicesUseCase
    {
        Task<IEnumerable<Invoice>> ExecuteAsync(int customerId);
    }
}
