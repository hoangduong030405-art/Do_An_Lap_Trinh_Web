using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IGlobalSearchRepository
    {
        Task<IEnumerable<GlobalSearchResultDto>> SearchAsync(string keyword, string role, int? customerId = null);
    }
}
