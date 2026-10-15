using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PublicPortal
{
    public interface IGetPublicStatisticsUseCase
    {
        Task<PublicStatisticsDto> ExecuteAsync();
    }
}
