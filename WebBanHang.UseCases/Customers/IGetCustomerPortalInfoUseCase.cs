using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Customers
{
    public interface IGetCustomerPortalInfoUseCase
    {
        Task<CustomerPortalInfoDto> ExecuteAsync(int customerId);
    }
}
