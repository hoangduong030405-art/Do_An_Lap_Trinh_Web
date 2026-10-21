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
        
        /// <summary>
        /// Xô chưa đến hạn (Trong hạn): OverdueDays == 0
        /// </summary>
        public decimal BucketNotDue { get; set; }

        /// <summary>
        /// Xô quá hạn 1-30 ngày
        /// </summary>
        public decimal Bucket1To30 { get; set; }

        /// <summary>
        /// Tương thích ngược: tổng xô chưa đến hạn + quá hạn 1-30 ngày
        /// </summary>
        public decimal Bucket0To30 
        { 
            get => BucketNotDue + Bucket1To30;
            set { } // Cho phép gán nếu có mapper
        }

        public decimal Bucket31To60 { get; set; }
        public decimal Bucket61To90 { get; set; }
        public decimal BucketOver90 { get; set; }
        
        /// <summary>
        /// Tổng nợ quá hạn thật sự (các xô từ 1 ngày trở lên)
        /// </summary>
        public decimal TotalOverdueDebt => Bucket1To30 + Bucket31To60 + Bucket61To90 + BucketOver90;

        public decimal TotalOutstandingDebt { get; set; }

        /// <summary>
        /// Kiểm soát toàn vẹn: Tổng tất cả các xô (Chưa đến hạn + 1-30 + 31-60 + 61-90 + >90) phải khớp 100% với TotalOutstandingDebt
        /// </summary>
        public bool IsIntegrityValid => Math.Abs((BucketNotDue + Bucket1To30 + Bucket31To60 + Bucket61To90 + BucketOver90) - TotalOutstandingDebt) < 0.01m;

        public List<InvoiceAgingItemDto> Invoices { get; set; } = new();
    }
}
