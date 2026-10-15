using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.PublicPortal
{
    public class GetPublicStatisticsUseCase : IGetPublicStatisticsUseCase
    {
        private readonly IPublicStatisticsRepository _publicStatisticsRepository;

        public GetPublicStatisticsUseCase(IPublicStatisticsRepository publicStatisticsRepository)
        {
            _publicStatisticsRepository = publicStatisticsRepository;
        }

        public async Task<PublicStatisticsDto> ExecuteAsync()
        {
            return await _publicStatisticsRepository.GetPublicStatisticsAsync();
        }
    }
}
