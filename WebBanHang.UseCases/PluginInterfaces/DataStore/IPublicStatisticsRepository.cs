using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IPublicStatisticsRepository
    {
        Task<PublicStatisticsDto> GetPublicStatisticsAsync();
    }
}
