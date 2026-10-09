using System;
using System.Collections.Generic;

namespace WebBanHang.UseCases.DTOs
{
    public class CustomerAgingReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        
        public decimal Bucket0To30 { get; set; }
        public decimal Bucket31To60 { get; set; }
        public decimal Bucket61To90 { get; set; }
        public decimal BucketOver90 { get; set; }
        public decimal TotalOutstandingDebt { get; set; }

        public bool IsIntegrityValid => Math.Abs((Bucket0To30 + Bucket31To60 + Bucket61To90 + BucketOver90) - TotalOutstandingDebt) < 0.01m;

        public List<InvoiceAgingItemDto> Invoices { get; set; } = new();
    }
}
