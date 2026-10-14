using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;
using WebBanHang.UseCases.Reports;

namespace WebBanHang.UseCases.Dashboard
{
    public class GetDashboardDataUseCase : IGetDashboardDataUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IGetCustomerAgingReportUseCase _agingReportUseCase;

        public GetDashboardDataUseCase(
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IPaymentRepository paymentRepository,
            IGetCustomerAgingReportUseCase agingReportUseCase)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _paymentRepository = paymentRepository;
            _agingReportUseCase = agingReportUseCase;
        }

        public async Task<DashboardSummaryDto> ExecuteAsync()
        {
            var invoices = (await _invoiceRepository.GetAllAsync()).ToList();
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var products = (await _productRepository.GetAllAsync()).ToList();
            var payments = (await _paymentRepository.GetAllAsync()).ToList();

            var summary = new DashboardSummaryDto
            {
                TotalRevenue = invoices.Sum(i => i.TotalAmount),
                TotalOutstandingDebt = invoices.Sum(i => i.Balance),
                TotalCollectedAmount = payments.Sum(p => p.Amount),
                OverdueInvoicesCount = invoices.Count(i => i.Balance > 0 && DateTime.Now > i.DueDate),
                TotalCustomersCount = customers.Count,
                TotalProductsCount = products.Count,
                TotalInvoicesCount = invoices.Count,
                TotalInventoryValuation = products.Sum(p => p.StockQuantity * p.UnitPrice),
                TopInventoryProducts = products
                    .OrderByDescending(p => p.StockQuantity * p.UnitPrice)
                    .Take(5)
                    .Select(p => new ProductInventoryDto
                    {
                        ProductId = p.Id,
                        ProductCode = p.ProductCode,
                        ProductName = p.Name,
                        Unit = p.Unit,
                        StockQuantity = p.StockQuantity,
                        UnitPrice = p.UnitPrice,
                        ImageUrl = p.ImageUrl
                    })
                    .ToList()
            };

            // Top Debtors
            var agingReports = (await _agingReportUseCase.ExecuteAsync(DateTime.Now)).ToList();
            summary.TopDebtors = agingReports
                .Where(r => r.TotalOutstandingDebt > 0)
                .OrderByDescending(r => r.TotalOutstandingDebt)
                .Take(5)
                .Select(r =>
                {
                    var cust = customers.FirstOrDefault(c => c.Id == r.CustomerId);
                    var maxOverdue = r.Invoices.Any() ? r.Invoices.Max(i => i.OverdueDays) : 0;
                    return new TopDebtorDto
                    {
                        CustomerId = r.CustomerId,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        CurrentDebt = r.TotalOutstandingDebt,
                        CreditLimit = cust?.CreditLimit ?? 0,
                        OverdueDays = maxOverdue > 0 ? maxOverdue : 0
                    };
                }).ToList();

            // Overall Aging 4 Buckets
            summary.OverallAging = new CustomerAgingReportDto
            {
                CustomerName = "Toàn hệ thống",
                Bucket0To30 = agingReports.Sum(r => r.Bucket0To30),
                Bucket31To60 = agingReports.Sum(r => r.Bucket31To60),
                Bucket61To90 = agingReports.Sum(r => r.Bucket61To90),
                BucketOver90 = agingReports.Sum(r => r.BucketOver90),
                TotalOutstandingDebt = agingReports.Sum(r => r.TotalOutstandingDebt)
            };

            // Top Selling Products from Dapper
            summary.TopProducts = (await _invoiceRepository.GetTopSellingProductsAsync(5)).ToList();

            // Monthly Revenue Stats from Dapper (12 months)
            var rawMonthly = (await _invoiceRepository.GetMonthlyRevenueStatisticsAsync(12)).ToList();
            var all12Months = new List<MonthlyRevenueDto>();
            for (int m = 1; m <= 12; m++)
            {
                var match = rawMonthly.FirstOrDefault(x => x.MonthNumber == m || x.MonthLabel.StartsWith($"{m:D2}/"));
                if (match != null)
                {
                    match.MonthNumber = m;
                    match.MonthLabel = $"Th {m}";
                    if (match.Cost == 0) match.Cost = match.Revenue * 0.72m;
                    all12Months.Add(match);
                }
                else
                {
                    all12Months.Add(new MonthlyRevenueDto
                    {
                        MonthNumber = m,
                        MonthLabel = $"Th {m}",
                        Revenue = 0,
                        Cost = 0,
                        Collected = 0
                    });
                }
            }
            summary.MonthlyRevenues = all12Months;

            return summary;
        }
    }
}
