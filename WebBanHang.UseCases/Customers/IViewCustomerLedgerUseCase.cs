using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Customers
{
    public interface IViewCustomerLedgerUseCase
    {
        Task<CustomerLedgerDto?> ExecuteAsync(int customerId, string? role = null, int? callerCustomerId = null);
    }
}
