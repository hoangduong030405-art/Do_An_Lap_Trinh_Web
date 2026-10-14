using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Products
{
    public class ManageProductUseCase : IManageProductUseCase
    {
        private readonly IProductRepository _productRepository;

        public ManageProductUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<int> AddProductAsync(ProductInputModel model)
        {
            var product = new Product
            {
                ProductCode = model.ProductCode,
                Name = model.Name,
                Unit = model.Unit,
                UnitPrice = model.UnitPrice,
                StockQuantity = model.StockQuantity,
                ImageUrl = model.ImageUrl,
                Description = model.Description
            };
            return await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(ProductInputModel model)
        {
            var product = new Product
            {
                Id = model.Id,
                ProductCode = model.ProductCode,
                Name = model.Name,
                Unit = model.Unit,
                UnitPrice = model.UnitPrice,
                StockQuantity = model.StockQuantity,
                ImageUrl = model.ImageUrl,
                Description = model.Description
            };
            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }
    }
}
