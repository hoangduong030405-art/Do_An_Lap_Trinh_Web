using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
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

        public async Task<DashboardSummaryDto> ExecuteAsync(string period = "this_month")
        {
            var now = DateTime.Now;
            var invoices = (await _invoiceRepository.GetAllAsync()).ToList();
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var products = (await _productRepository.GetAllAsync()).ToList();
            var payments = (await _paymentRepository.GetAllAsync()).ToList();

            // 1. Xác định khung thời gian kỳ hiện tại và kỳ trước cùng độ dài
            DateTime currentStart, currentEnd, prevStart, prevEnd;
            if (period == "this_quarter")
            {
                int currentQuarter = (now.Month - 1) / 3 + 1;
                currentStart = new DateTime(now.Year, (currentQuarter - 1) * 3 + 1, 1);
                currentEnd = currentStart.AddMonths(3);
                prevStart = currentStart.AddMonths(-3);
                prevEnd = currentStart;
            }
            else if (period == "this_year")
            {
                currentStart = new DateTime(now.Year, 1, 1);
                currentEnd = currentStart.AddYears(1);
                prevStart = currentStart.AddYears(-1);
                prevEnd = currentStart;
            }
            else // default: "this_month"
            {
                currentStart = new DateTime(now.Year, now.Month, 1);
                currentEnd = currentStart.AddMonths(1);
                prevStart = currentStart.AddMonths(-1);
                prevEnd = currentStart;
            }

            // Hóa đơn & Thu tiền trong kỳ báo cáo
            var invoicesInPeriod = invoices
                .Where(i => i.InvoiceDate >= currentStart && i.InvoiceDate < currentEnd)
                .ToList();
            var invoicesInPrevPeriod = invoices
                .Where(i => i.InvoiceDate >= prevStart && i.InvoiceDate < prevEnd)
                .ToList();
            var paymentsInPeriod = payments
                .Where(p => p.PaymentDate >= currentStart && p.PaymentDate < currentEnd)
                .ToList();

            var summary = new DashboardSummaryDto
            {
                TotalRevenue = invoicesInPeriod.Sum(i => i.TotalAmount),
                TotalInvoicesCount = invoicesInPeriod.Count,
                TotalCollectedAmount = paymentsInPeriod.Sum(p => p.Amount),
                TotalCustomersCount = customers.Count,
                TotalProductsCount = products.Count,
                TotalInventoryCost = products.Sum(p => p.StockQuantity * p.CostPrice),
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
                        CostPrice = p.CostPrice,
                        ImageUrl = p.ImageUrl
                    })
                    .ToList()
            };

            // So sánh kỳ trước thật sự:
            var prevRevenue = invoicesInPrevPeriod.Sum(i => i.TotalAmount);
            if (prevRevenue > 0)
            {
                var diffPercent = Math.Round(((summary.TotalRevenue - prevRevenue) / prevRevenue) * 100, 1);
                summary.PreviousPeriodComparisonText = diffPercent >= 0 
                    ? $"+{diffPercent:N1}% so kỳ trước" 
                    : $"{diffPercent:N1}% so kỳ trước";
            }
            else
            {
                summary.PreviousPeriodComparisonText = "Chưa có dữ liệu kỳ trước";
            }

            // 2. Tuổi nợ toàn hệ thống (Single Source of Truth từ GetCustomerAgingReportUseCase)
            var agingReports = (await _agingReportUseCase.ExecuteAsync(now)).ToList();
            summary.OverallAging = new CustomerAgingReportDto
            {
                CustomerName = "Toàn hệ thống",
                BucketNotDue = agingReports.Sum(r => r.BucketNotDue),
                Bucket1To30 = agingReports.Sum(r => r.Bucket1To30),
                Bucket31To60 = agingReports.Sum(r => r.Bucket31To60),
                Bucket61To90 = agingReports.Sum(r => r.Bucket61To90),
                BucketOver90 = agingReports.Sum(r => r.BucketOver90),
                TotalOutstandingDebt = agingReports.Sum(r => r.TotalOutstandingDebt)
            };

            summary.TotalOutstandingDebt = summary.OverallAging.TotalOutstandingDebt;
            summary.OverdueDebt = summary.OverallAging.TotalOverdueDebt;
            summary.CurrentDebt = summary.OverallAging.BucketNotDue;
            summary.TotalDebtorsCount = agingReports.Count(r => r.TotalOutstandingDebt > 0);

            // Số hóa đơn quá hạn: dùng đúng hàm Domain Invoice.GetOverdueDays(now) > 0
            summary.OverdueInvoicesCount = invoices.Count(i => 
                i.Status != InvoiceStatus.Paid && 
                i.Status != InvoiceStatus.Cancelled && 
                i.GetOverdueDays(now) > 0);

            // Top Debtors
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

            // Tỷ lệ tập trung công nợ Top 5 KH
            summary.Top5DebtorsPercentage = summary.TotalOutstandingDebt > 0 
                ? Math.Round((summary.TopDebtors.Sum(d => d.CurrentDebt) / summary.TotalOutstandingDebt) * 100, 1) 
                : 0m;

            // Top Selling Products từ Dapper
            summary.TopProducts = (await _invoiceRepository.GetTopSellingProductsAsync(5)).ToList();

            // Doanh thu, Chi phí, Lợi nhuận 12 tháng từ Dapper (XÓA HỆ SỐ 0.72)
            var rawMonthly = (await _invoiceRepository.GetMonthlyRevenueStatisticsAsync(12)).ToList();
            var all12Months = new List<MonthlyRevenueDto>();
            for (int m = 1; m <= 12; m++)
            {
                var match = rawMonthly.FirstOrDefault(x => x.MonthNumber == m || x.MonthLabel.StartsWith($"{m:D2}/"));
                if (match != null)
                {
                    match.MonthNumber = m;
                    match.MonthLabel = $"Th {m}";
                    // Không áp hệ số 0.72 - Giá vốn phản ánh thật từ InvoiceLines
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

            // Giá vốn thực tế trong kỳ tương ứng
            if (period == "this_quarter")
            {
                int currentQuarter = (now.Month - 1) / 3 + 1;
                summary.TotalActualCost = all12Months
                    .Where(m => (m.MonthNumber - 1) / 3 + 1 == currentQuarter)
                    .Sum(m => m.Cost);
            }
            else if (period == "this_year")
            {
                summary.TotalActualCost = all12Months.Sum(m => m.Cost);
            }
            else // "this_month"
            {
                summary.TotalActualCost = all12Months
                    .FirstOrDefault(m => m.MonthNumber == now.Month)?.Cost ?? 0m;
            }

            return summary;
        }
    }
}
