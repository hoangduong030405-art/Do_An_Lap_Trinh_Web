using System.Threading.Tasks;

namespace WebBanHang.UseCases.Customers
{
    public interface IGetCustomerDebtUseCase
    {
        Task<decimal> ExecuteAsync(int customerId);
    }
}
