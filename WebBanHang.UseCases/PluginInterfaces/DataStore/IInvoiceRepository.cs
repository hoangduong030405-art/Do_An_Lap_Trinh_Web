using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<IEnumerable<Invoice>> GetUnpaidInvoicesByCustomerOrderedByDateAsync(int customerId);
        Task<int> CreateInvoiceWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines);
    }
}
