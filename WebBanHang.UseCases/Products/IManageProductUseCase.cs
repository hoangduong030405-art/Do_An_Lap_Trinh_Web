using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Products
{
    public interface IManageProductUseCase
    {
        Task<int> AddProductAsync(ProductInputModel model);
        Task UpdateProductAsync(ProductInputModel model);
        Task DeleteProductAsync(int id);
    }
}
