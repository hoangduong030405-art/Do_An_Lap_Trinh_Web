using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Customer>("SELECT * FROM Customers");
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Customer>("SELECT * FROM Customers WHERE Id = @Id", new { Id = id });
        }

        public async Task<decimal> GetTotalOutstandingDebtAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT ISNULL(SUM(Balance), 0) FROM Invoices WHERE CustomerId = @CustomerId AND Balance > 0 AND Status NOT IN ('Cancelled', '4')";
            return await connection.ExecuteScalarAsync<decimal>(query, new { CustomerId = customerId });
        }

        public async Task<int> AddAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO Customers (CustomerCode, FullName, PhoneNumber, Address, CreditLimit, PaymentTermsDays) OUTPUT INSERTED.Id VALUES (@CustomerCode, @FullName, @PhoneNumber, @Address, @CreditLimit, @PaymentTermsDays)";
            return await connection.QuerySingleAsync<int>(query, customer);
        }

        public async Task UpdateAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "UPDATE Customers SET FullName = @FullName, PhoneNumber = @PhoneNumber, Address = @Address, CreditLimit = @CreditLimit, PaymentTermsDays = @PaymentTermsDays WHERE Id = @Id";
            await connection.ExecuteAsync(query, customer);
        }
    }
}
