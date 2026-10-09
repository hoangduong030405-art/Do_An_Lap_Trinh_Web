using System;

namespace WebBanHang.UseCases.DTOs
{
    public class InvoiceAgingItemDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public int OverdueDays { get; set; }
        public string AgingCategory { get; set; } = string.Empty;
    }
}
