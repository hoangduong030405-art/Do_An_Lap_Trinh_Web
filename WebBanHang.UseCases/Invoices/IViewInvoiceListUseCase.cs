using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Invoices
{
    public interface IViewInvoiceListUseCase
    {
        Task<IEnumerable<InvoiceListDto>> ExecuteAsync(string? keyword = null, string? status = null);
    }
}
