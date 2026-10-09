using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.Customers
{
    public interface IManageCustomerUseCase
    {
        Task<int> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
    }
}
