using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class ViewInvoiceDetailUseCase : IViewInvoiceDetailUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public ViewInvoiceDetailUseCase(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceDetailDto?> ExecuteAsync(int id)
        {
            return await _invoiceRepository.GetInvoiceDetailByIdAsync(id);
        }
    }
}
