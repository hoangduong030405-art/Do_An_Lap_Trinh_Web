using System;
using System.Collections.Generic;

namespace WebBanHang.UseCases.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalOutstandingDebt { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public decimal OverdueDebt { get; set; }
        public decimal CurrentDebt { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public int TotalProductsCount { get; set; }
        public int TotalInvoicesCount { get; set; }
        public int TotalDebtorsCount { get; set; }
        public decimal Top5DebtorsPercentage { get; set; }
        public string PreviousPeriodComparisonText { get; set; } = "Chưa có dữ liệu kỳ trước";

        public decimal TotalActualCost { get; set; }
        public decimal TotalActualProfit => TotalRevenue - TotalActualCost;
        public decimal ActualProfitMargin => TotalRevenue > 0 ? Math.Round((TotalActualProfit / TotalRevenue) * 100, 1) : 0;

        public decimal CollectionRate => TotalRevenue > 0 ? Math.Min(100, Math.Round((TotalCollectedAmount / TotalRevenue) * 100, 1)) : 0;
        public decimal OverduePercent => TotalOutstandingDebt > 0 ? Math.Round((OverdueDebt / TotalOutstandingDebt) * 100, 1) : 0;

        public decimal TotalInventoryCost { get; set; }
        public decimal TotalInventoryValuation { get; set; }
        public decimal TotalExpectedInventoryProfit => TotalInventoryValuation - TotalInventoryCost;
        public decimal ExpectedProfitMargin => TotalInventoryValuation > 0 ? Math.Round((TotalExpectedInventoryProfit / TotalInventoryValuation) * 100, 1) : 0;

        public List<ProductInventoryDto> TopInventoryProducts { get; set; } = new();

        public List<TopDebtorDto> TopDebtors { get; set; } = new();
        public List<TopSellingProductDto> TopProducts { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new();
        public CustomerAgingReportDto OverallAging { get; set; } = new();
    }

    public class ProductInventoryDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost => StockQuantity * CostPrice;
        public decimal Valuation => StockQuantity * UnitPrice;
        public decimal ExpectedProfit => Valuation - TotalCost;
        public decimal ProfitMargin => UnitPrice > 0 ? Math.Round(((UnitPrice - CostPrice) / UnitPrice) * 100, 1) : 0;
        public string ImageUrl { get; set; } = string.Empty;
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
        public int MonthNumber { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Collected { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Revenue - Cost;
    }
}
