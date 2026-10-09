using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Payments
{
    public class ViewPaymentHistoryUseCase : IViewPaymentHistoryUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public ViewPaymentHistoryUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentHistoryDto>> ExecuteAsync()
        {
            return await _paymentRepository.GetPaymentHistoryWithDetailsAsync();
        }
    }
}
