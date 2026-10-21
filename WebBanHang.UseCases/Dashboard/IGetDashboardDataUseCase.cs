using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Dashboard
{
    public interface IGetDashboardDataUseCase
    {
        Task<DashboardSummaryDto> ExecuteAsync(string period = "this_month");
    }
}
