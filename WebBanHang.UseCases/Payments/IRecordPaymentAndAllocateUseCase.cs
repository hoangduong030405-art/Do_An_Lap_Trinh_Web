using System;
using System.Threading.Tasks;

namespace WebBanHang.UseCases.Payments
{
    public interface IRecordPaymentAndAllocateUseCase
    {
        Task<int> ExecuteAsync(int customerId, decimal paymentAmount, DateTime paymentDate, string? note);
    }
}
