using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Invoices
{
    public interface IViewInvoiceDetailUseCase
    {
        Task<InvoiceDetailDto?> ExecuteAsync(int id, string? role = null, int? customerId = null);
    }
}
