using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.Customers
{
    public interface IViewCustomerListUseCase
    {
        Task<IEnumerable<Customer>> ExecuteAsync();
    }
}
