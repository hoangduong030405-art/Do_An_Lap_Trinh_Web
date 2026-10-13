using System.Threading.Tasks;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Customers
{
    public class GetCustomerDebtUseCase : IGetCustomerDebtUseCase
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerDebtUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<decimal> ExecuteAsync(int customerId)
        {
            return await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
        }
    }
}
