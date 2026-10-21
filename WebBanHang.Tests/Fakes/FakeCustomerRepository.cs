using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Tests.Fakes
{
    public class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Customers { get; } = new();
        public Dictionary<int, decimal> OutstandingDebts { get; } = new();

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Customer>>(Customers);
        }

        public Task<Customer?> GetByIdAsync(int id)
        {
            var customer = Customers.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(customer);
        }

        public Task<decimal> GetTotalOutstandingDebtAsync(int customerId)
        {
            if (OutstandingDebts.TryGetValue(customerId, out var debt))
            {
                return Task.FromResult(debt);
            }
            return Task.FromResult(0m);
        }

        public Task<int> AddAsync(Customer customer)
        {
            customer.Id = Customers.Any() ? Customers.Max(c => c.Id) + 1 : 1;
            Customers.Add(customer);
            return Task.FromResult(customer.Id);
        }

        public Task UpdateAsync(Customer customer)
        {
            var existing = Customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existing != null)
            {
                Customers.Remove(existing);
                Customers.Add(customer);
            }
            return Task.CompletedTask;
        }
    }
}
