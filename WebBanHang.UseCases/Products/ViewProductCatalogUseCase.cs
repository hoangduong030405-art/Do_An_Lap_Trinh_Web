using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Products
{
    public class ViewProductCatalogUseCase : IViewProductCatalogUseCase
    {
        private readonly IProductRepository _productRepository;

        public ViewProductCatalogUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> ExecuteAsync(string keyword = "")
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await _productRepository.GetAllAsync();
            }
            return await _productRepository.SearchAsync(keyword);
        }
    }
}
