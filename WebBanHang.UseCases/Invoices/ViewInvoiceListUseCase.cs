using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class ViewInvoiceListUseCase : IViewInvoiceListUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public ViewInvoiceListUseCase(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<InvoiceListDto>> ExecuteAsync(string? keyword = null, string? status = null)
        {
            return await _invoiceRepository.GetInvoicesWithCustomerAsync(keyword, status);
        }
    }
}
