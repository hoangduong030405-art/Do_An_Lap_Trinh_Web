using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.GlobalSearch
{
    public interface IGlobalSearchUseCase
    {
        Task<IEnumerable<GlobalSearchResultDto>> ExecuteAsync(string keyword, string role, int? customerId = null);
    }

    public class GlobalSearchUseCase : IGlobalSearchUseCase
    {
        private readonly IGlobalSearchRepository _repository;

        public GlobalSearchUseCase(IGlobalSearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<GlobalSearchResultDto>> ExecuteAsync(string keyword, string role, int? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            {
                return new List<GlobalSearchResultDto>();
            }

            return await _repository.SearchAsync(keyword.Trim(), role, customerId);
        }
    }
}
