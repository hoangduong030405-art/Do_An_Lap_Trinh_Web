using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Invoices
{
    public interface ICreateCustomerPortalOrderUseCase
    {
        Task<CustomerPortalOrderResultDto> ExecuteAsync(int customerId, List<InvoiceLine> items);
    }
}
