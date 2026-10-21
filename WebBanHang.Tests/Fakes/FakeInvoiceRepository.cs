using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.Tests.Fakes
{
    public class FakeInvoiceRepository : IInvoiceRepository
    {
        public List<Invoice> Invoices { get; } = new();
        public List<InvoiceLine> InvoiceLines { get; } = new();

        public Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Invoice>>(Invoices);
        }

        public Task<Invoice?> GetByIdAsync(int id)
        {
            var invoice = Invoices.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(invoice);
        }

        public Task<IEnumerable<Invoice>> GetUnpaidInvoicesByCustomerOrderedByDateAsync(int customerId)
        {
            // Trả dữ liệu thô nguyên bản theo thứ tự lưu trữ trong danh sách, KHÔNG tự động OrderBy hay lọc nghiệp vụ
            var result = Invoices.Where(i => i.CustomerId == customerId).ToList();
            return Task.FromResult<IEnumerable<Invoice>>(result);
        }

        public Task<int> CreateInvoiceWithLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> lines)
        {
            invoice.Id = Invoices.Any() ? Invoices.Max(i => i.Id) + 1 : 1;
            Invoices.Add(invoice);

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                line.Id = InvoiceLines.Any() ? InvoiceLines.Max(l => l.Id) + 1 : 1;
                InvoiceLines.Add(line);
            }

            return Task.FromResult(invoice.Id);
        }

        public List<InvoiceListDto> CustomInvoicesWithCustomer { get; } = new();

        public Task<IEnumerable<InvoiceListDto>> GetInvoicesWithCustomerAsync(string? keyword = null, string? status = null)
        {
            if (CustomInvoicesWithCustomer.Any())
            {
                return Task.FromResult<IEnumerable<InvoiceListDto>>(CustomInvoicesWithCustomer);
            }

            var list = Invoices.Select(i => new InvoiceListDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerId = i.CustomerId,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Balance = i.Balance,
                Status = i.Status
            }).ToList();

            return Task.FromResult<IEnumerable<InvoiceListDto>>(list);
        }

        public Task<InvoiceDetailDto?> GetInvoiceDetailByIdAsync(int id)
        {
            var invoice = Invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return Task.FromResult<InvoiceDetailDto?>(null);

            var dto = new InvoiceDetailDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                Balance = invoice.Balance,
                Status = invoice.Status
            };
            return Task.FromResult<InvoiceDetailDto?>(dto);
        }

        public Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId)
        {
            var result = Invoices.Where(i => i.CustomerId == customerId);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<TopSellingProductDto>> GetTopSellingProductsAsync(int top = 5)
        {
            return Task.FromResult<IEnumerable<TopSellingProductDto>>(new List<TopSellingProductDto>());
        }

        public Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueStatisticsAsync(int months = 6)
        {
            return Task.FromResult<IEnumerable<MonthlyRevenueDto>>(new List<MonthlyRevenueDto>());
        }
    }
}
