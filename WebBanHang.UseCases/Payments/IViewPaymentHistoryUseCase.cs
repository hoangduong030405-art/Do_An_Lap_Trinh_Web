using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Payments
{
    public interface IViewPaymentHistoryUseCase
    {
        Task<IEnumerable<PaymentHistoryDto>> ExecuteAsync();
    }
}
