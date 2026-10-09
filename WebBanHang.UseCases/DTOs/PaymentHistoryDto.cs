using System;
using System.Collections.Generic;

namespace WebBanHang.UseCases.DTOs
{
    public class PaymentHistoryDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public string Note { get; set; } = string.Empty;

        public List<PaymentAllocationDetailDto> Allocations { get; set; } = new();
    }

    public class PaymentAllocationDetailDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal AllocatedAmount { get; set; }
    }

    public class CustomerLedgerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal TotalDebit { get; set; }    // Tổng tiền mua hàng (Hóa đơn)
        public decimal TotalCredit { get; set; }   // Tổng tiền đã trả (Phiếu thu)
        public decimal EndingBalance => TotalDebit - TotalCredit; // Dư nợ cuối kỳ

        public List<LedgerEntryDto> Entries { get; set; } = new();
    }

    public class LedgerEntryDto
    {
        public DateTime TransactionDate { get; set; }
        public string DocumentType { get; set; } = string.Empty; // "Hóa Đơn" hoặc "Phiếu Thu"
        public string DocumentNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }     // Phát sinh nợ (Tăng)
        public decimal Credit { get; set; }    // Phát sinh có (Giảm)
        public decimal RunningBalance { get; set; } // Dư nợ lũy kế
    }
}
