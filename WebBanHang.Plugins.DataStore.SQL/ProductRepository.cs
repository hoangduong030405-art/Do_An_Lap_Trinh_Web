using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Product>("SELECT * FROM Products ORDER BY Name ASC");
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Products WHERE Name LIKE @Keyword OR ProductCode LIKE @Keyword ORDER BY Name ASC";
            return await connection.QueryAsync<Product>(query, new { Keyword = $"%{keyword}%" });
        }

        public async Task<int> AddAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Products (ProductCode, Name, Unit, UnitPrice, StockQuantity, ImageUrl, Description)
                OUTPUT INSERTED.Id
                VALUES (@ProductCode, @Name, @Unit, @UnitPrice, @StockQuantity, @ImageUrl, @Description)";
            return await connection.QuerySingleAsync<int>(sql, product);
        }

        public async Task UpdateAsync(Product product)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Products 
                SET ProductCode = @ProductCode, 
                    Name = @Name, 
                    Unit = @Unit, 
                    UnitPrice = @UnitPrice, 
                    StockQuantity = @StockQuantity, 
                    Description = @Description
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "DELETE FROM Products WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
