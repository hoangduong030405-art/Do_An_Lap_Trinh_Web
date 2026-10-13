using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.PluginInterfaces.DataStore
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<IEnumerable<Invoice>> GetUnpaidInvoicesByCustomerOrderedByDateAsync(int customerId);
        Task<int> CreateInvoiceWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines);
        Task<IEnumerable<InvoiceListDto>> GetInvoicesWithCustomerAsync(string? keyword = null, string? status = null);
        Task<InvoiceDetailDto?> GetInvoiceDetailByIdAsync(int id);
        Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId);
        Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int top = 5);
        Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueStatisticsAsync(int months = 6);
    }
}
