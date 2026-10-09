using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Customers
{
    public class ViewCustomerListUseCase : IViewCustomerListUseCase
    {
        private readonly ICustomerRepository _customerRepository;

        public ViewCustomerListUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> ExecuteAsync()
        {
            return await _customerRepository.GetAllAsync();
        }
    }
}
