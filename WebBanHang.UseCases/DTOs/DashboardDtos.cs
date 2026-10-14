using System;
using System.Collections.Generic;

namespace WebBanHang.UseCases.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalOutstandingDebt { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public int TotalProductsCount { get; set; }
        public int TotalInvoicesCount { get; set; }

        public List<TopDebtorDto> TopDebtors { get; set; } = new();
        public List<TopSellingProductDto> TopProducts { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new();
        public CustomerAgingReportDto OverallAging { get; set; } = new();
    }

    public class TopDebtorDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal CurrentDebt { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal DebtRatio => CreditLimit > 0 ? (CurrentDebt / CreditLimit) * 100 : 100;
        public int OverdueDays { get; set; }
    }

    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Collected { get; set; }
    }
}
