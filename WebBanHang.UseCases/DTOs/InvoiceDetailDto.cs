using System;
using System.Collections.Generic;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.DTOs
{
    public class InvoiceListDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public InvoiceStatus Status { get; set; }
        public int OverdueDays => Balance > 0 && DateTime.Now > DueDate ? (DateTime.Now - DueDate).Days : 0;
    }

    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public InvoiceStatus Status { get; set; }

        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;

        public List<InvoiceLineDetailDto> Lines { get; set; } = new();

        public decimal TotalCost => Lines.Sum(l => l.LineCost);
        public decimal TotalProfit => TotalAmount - TotalCost;
        public decimal ProfitMargin => TotalAmount > 0 ? Math.Round((TotalProfit / TotalAmount) * 100, 1) : 0;
    }

    public class InvoiceLineDetailDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public decimal LineCost => Quantity * CostPrice;
        public decimal LineProfit => LineTotal - LineCost;
        public decimal ProfitMargin => UnitPrice > 0 ? Math.Round(((UnitPrice - CostPrice) / UnitPrice) * 100, 1) : 0;
    }
}
