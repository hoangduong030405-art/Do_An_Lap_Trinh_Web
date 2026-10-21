using System;

namespace WebBanHang.CoreBusiness.Entities
{
    public class Invoice
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

        /// <summary>
        /// Tính số ngày quá hạn theo chuẩn duy nhất:
        /// - Nếu còn nợ (Balance > 0) và asOfDate.Date > DueDate.Date: trả về số ngày trễ (> 0)
        /// - Đúng ngày DueDate hoặc chưa đến hạn, hoặc đã hết nợ: trả về 0 (chưa quá hạn)
        /// </summary>
        public static int CalculateOverdueDays(DateTime dueDate, decimal balance, DateTime asOfDate)
        {
            if (balance <= 0) return 0;
            var days = (asOfDate.Date - dueDate.Date).Days;
            return days > 0 ? days : 0;
        }

        public static bool CheckIsOverdue(DateTime dueDate, decimal balance, DateTime asOfDate)
            => CalculateOverdueDays(dueDate, balance, asOfDate) > 0;

        public int GetOverdueDays(DateTime asOfDate)
            => CalculateOverdueDays(DueDate, Balance, asOfDate);

        public bool IsOverdue(DateTime asOfDate)
            => GetOverdueDays(asOfDate) > 0;
    }
}
