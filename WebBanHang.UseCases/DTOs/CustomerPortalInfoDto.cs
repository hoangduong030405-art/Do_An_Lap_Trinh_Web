using System;

namespace WebBanHang.UseCases.DTOs
{
    public class CustomerPortalInfoDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal AvailableCredit => Math.Max(0, CreditLimit - CurrentDebt);
        public decimal CreditUtilizationPercent => CreditLimit > 0 ? Math.Min(100, Math.Round((CurrentDebt / CreditLimit) * 100, 1)) : 0;
        public bool IsCreditLimitReached => CurrentDebt >= CreditLimit;
        public int TotalInvoicesCount { get; set; }
        public int UnpaidInvoicesCount { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public decimal OverdueDebtAmount { get; set; }
    }
}
