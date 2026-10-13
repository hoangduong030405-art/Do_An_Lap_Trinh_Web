using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class GetUnpaidInvoicesUseCase : IGetUnpaidInvoicesUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetUnpaidInvoicesUseCase(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<Invoice>> ExecuteAsync(int customerId)
        {
            if (customerId <= 0) return new List<Invoice>();
            return await _invoiceRepository.GetUnpaidInvoicesByCustomerOrderedByDateAsync(customerId);
        }
    }
}
