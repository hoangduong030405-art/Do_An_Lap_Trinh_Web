using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class PublicStatisticsRepository : IPublicStatisticsRepository
    {
        private readonly string _connectionString;

        public PublicStatisticsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new System.InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<PublicStatisticsDto> GetPublicStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            const string query = @"
                SELECT 
                    (SELECT COUNT(*) FROM Products) AS TotalProducts,
                    (SELECT COUNT(*) FROM Customers) AS TotalCustomers,
                    (SELECT COUNT(*) FROM Invoices) AS TotalInvoicesProcessed,
                    COALESCE((SELECT SUM(TotalAmount) FROM Invoices), 0) AS TotalRevenue,
                    COALESCE((SELECT SUM(PaidAmount) FROM Invoices), 0) AS TotalCollectedAmount;
            ";

            var result = await connection.QuerySingleOrDefaultAsync<PublicStatisticsDto>(query);
            if (result == null)
            {
                return new PublicStatisticsDto();
            }

            if (result.TotalRevenue > 0)
            {
                result.DebtCollectionRate = System.Math.Round((result.TotalCollectedAmount / result.TotalRevenue) * 100, 1);
            }
            else
            {
                result.DebtCollectionRate = 0m;
            }

            return result;
        }
    }
}
