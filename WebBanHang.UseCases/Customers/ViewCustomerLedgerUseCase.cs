using System;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Customers
{
    public class ViewCustomerLedgerUseCase : IViewCustomerLedgerUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public ViewCustomerLedgerUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<CustomerLedgerDto?> ExecuteAsync(int customerId, string? role = null, int? callerCustomerId = null)
        {
            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                if (!callerCustomerId.HasValue || customerId != callerCustomerId.Value)
                {
                    return null;
                }
            }

            return await _paymentRepository.GetCustomerLedgerAsync(customerId);
        }
    }
}
