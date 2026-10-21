using System;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.DTOs
{
    public class CustomerPortalInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public InvoiceStatus Status { get; set; }

        public bool IsOverdue => Invoice.CheckIsOverdue(DueDate, Balance, DateTime.Today);
        public int OverdueDays => Invoice.CalculateOverdueDays(DueDate, Balance, DateTime.Today);
        public int RemainingDays => (!IsOverdue && Balance > 0) ? Math.Max(0, (DueDate.Date - DateTime.Today).Days) : 0;
    }
}
