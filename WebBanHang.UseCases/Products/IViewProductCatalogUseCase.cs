using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.Products
{
    public interface IViewProductCatalogUseCase
    {
        Task<IEnumerable<Product>> ExecuteAsync(string keyword = "");
    }
}
