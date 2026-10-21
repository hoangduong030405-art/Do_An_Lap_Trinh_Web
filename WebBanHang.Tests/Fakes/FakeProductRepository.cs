using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Tests.Fakes
{
    public class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; } = new();

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Product>>(Products);
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }

        public Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            var results = Products.Where(p => 
                p.Name.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                p.ProductCode.Contains(keyword, System.StringComparison.OrdinalIgnoreCase));
            return Task.FromResult<IEnumerable<Product>>(results);
        }

        public Task<int> AddAsync(Product product)
        {
            product.Id = Products.Any() ? Products.Max(p => p.Id) + 1 : 1;
            Products.Add(product);
            return Task.FromResult(product.Id);
        }

        public Task UpdateAsync(Product product)
        {
            var existing = Products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                Products.Remove(existing);
                Products.Add(product);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var existing = Products.FirstOrDefault(p => p.Id == id);
            if (existing != null)
            {
                Products.Remove(existing);
            }
            return Task.CompletedTask;
        }
    }
}
