using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<decimal> GetTotalOutstandingDebtAsync(int customerId);
        Task<int> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
    }
}
