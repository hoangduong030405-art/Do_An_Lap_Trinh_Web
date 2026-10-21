using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.Tests.Fakes;
using WebBanHang.UseCases.Reports;
using Xunit;

namespace WebBanHang.Tests
{
    public class DebtAgingReportTests
    {
        private readonly FakeCustomerRepository _customerRepo = new();
        private readonly FakeInvoiceRepository _invoiceRepo = new();
        private readonly GetCustomerAgingReportUseCase _useCase;

        public DebtAgingReportTests()
        {
            _useCase = new GetCustomerAgingReportUseCase(_customerRepo, _invoiceRepo);
        }

        [Fact(DisplayName = "Tuổi nợ(i): Tổng tất cả 5 xô (Chưa đến hạn + 1-30 + 31-60 + 61-90 + >90) luôn luôn bằng đúng Tổng Balance nợ (IsIntegrityValid == true)")]
        public async Task GetCustomerAgingReport_SumOfAllFiveBuckets_EqualsTotalOutstandingBalance()
        {
            // Arrange: 1 khách hàng có 5 hóa đơn rơi vào đúng 5 xô tuổi nợ khác nhau
            var asOfDate = new DateTime(2026, 10, 20);

            var customer = new Customer { Id = 1, CustomerCode = "KH001", FullName = "Tạp Hóa Bà Ba" };
            _customerRepo.Customers.Add(customer);

            _invoiceRepo.Invoices.AddRange(new[]
            {
                // Xô Chưa đến hạn: DueDate = 2026-10-25 (còn 5 ngày nữa mới đến hạn)
                new Invoice { Id = 1, CustomerId = 1, InvoiceNumber = "H1", DueDate = new DateTime(2026, 10, 25), Balance = 1_000_000m, Status = InvoiceStatus.Unpaid },
                
                // Xô 1-30 ngày: DueDate = 2026-10-05 (quá hạn 15 ngày)
                new Invoice { Id = 2, CustomerId = 1, InvoiceNumber = "H2", DueDate = new DateTime(2026, 10, 5), Balance = 2_000_000m, Status = InvoiceStatus.Unpaid },
                
                // Xô 31-60 ngày: DueDate = 2026-09-05 (quá hạn 45 ngày)
                new Invoice { Id = 3, CustomerId = 1, InvoiceNumber = "H3", DueDate = new DateTime(2026, 9, 5), Balance = 3_000_000m, Status = InvoiceStatus.Unpaid },
                
                // Xô 61-90 ngày: DueDate = 2026-08-11 (quá hạn 70 ngày)
                new Invoice { Id = 4, CustomerId = 1, InvoiceNumber = "H4", DueDate = new DateTime(2026, 8, 11), Balance = 4_000_000m, Status = InvoiceStatus.Unpaid },
                
                // Xô >90 ngày: DueDate = 2026-07-02 (quá hạn 110 ngày)
                new Invoice { Id = 5, CustomerId = 1, InvoiceNumber = "H5", DueDate = new DateTime(2026, 7, 2), Balance = 5_000_000m, Status = InvoiceStatus.Unpaid }
            });

            // Act
            var reports = (await _useCase.ExecuteAsync(asOfDate)).ToList();

            // Assert
            Assert.Single(reports);
            var report = reports[0];

            Assert.Equal(1_000_000m, report.BucketNotDue);
            Assert.Equal(2_000_000m, report.Bucket1To30);
            Assert.Equal(3_000_000m, report.Bucket31To60);
            Assert.Equal(4_000_000m, report.Bucket61To90);
            Assert.Equal(5_000_000m, report.BucketOver90);

            decimal sumOfBuckets = report.BucketNotDue + report.Bucket1To30 + report.Bucket31To60 + report.Bucket61To90 + report.BucketOver90;
            decimal expectedTotal = 15_000_000m;

            Assert.Equal(expectedTotal, report.TotalOutstandingDebt);
            Assert.Equal(expectedTotal, sumOfBuckets);
            Assert.True(report.IsIntegrityValid);
        }

        [Fact(DisplayName = "Tuổi nợ(j): Hóa đơn có ngày tham chiếu đúng bằng DueDate (asOfDate.Date == DueDate.Date) thì CHƯA tính quá hạn (xếp vào BucketNotDue)")]
        public async Task GetCustomerAgingReport_InvoiceOnExactDueDate_IsNotOverdue_PlacedInBucketNotDue()
        {
            // Arrange
            var asOfDate = new DateTime(2026, 10, 20);

            var customer = new Customer { Id = 1, CustomerCode = "KH001", FullName = "Tạp Hóa Bà Ba" };
            _customerRepo.Customers.Add(customer);

            // Hóa đơn đến hạn vào đúng ngày 2026-10-20
            var invoice = new Invoice
            {
                Id = 10,
                CustomerId = 1,
                InvoiceNumber = "H-EXACT",
                DueDate = new DateTime(2026, 10, 20),
                Balance = 8_000_000m,
                Status = InvoiceStatus.Unpaid
            };
            _invoiceRepo.Invoices.Add(invoice);

            // Act
            var reports = (await _useCase.ExecuteAsync(asOfDate)).ToList();

            // Assert
            var report = reports.Single();
            Assert.Equal(8_000_000m, report.BucketNotDue);     // Xô Chưa đến hạn
            Assert.Equal(0m, report.Bucket1To30);             // Không lọt vào quá hạn
            Assert.Equal(0m, report.TotalOverdueDebt);         // Tổng quá hạn = 0
            Assert.Equal(0, invoice.GetOverdueDays(asOfDate)); // Hàm domain trả về 0 ngày
            Assert.False(invoice.IsOverdue(asOfDate));         // Chưa quá hạn
        }

        [Fact(DisplayName = "Tuổi nợ(k): Hóa đơn có trạng thái Paid (hoặc Cancelled, hoặc Balance = 0) tuyệt đối không xuất hiện trong báo cáo")]
        public async Task GetCustomerAgingReport_PaidAndCancelledInvoices_AreExcludedFromReport()
        {
            // Arrange: Khách hàng có 1 HĐ đã thanh toán (Paid, Balance = 0), 1 HĐ bị hủy (Cancelled)
            var customer = new Customer { Id = 1, CustomerCode = "KH001", FullName = "Tạp Hóa Bà Ba" };
            _customerRepo.Customers.Add(customer);

            _invoiceRepo.Invoices.AddRange(new[]
            {
                new Invoice { Id = 1, CustomerId = 1, InvoiceNumber = "H-PAID", DueDate = new DateTime(2026, 9, 1), Balance = 0m, PaidAmount = 5_000_000m, Status = InvoiceStatus.Paid },
                new Invoice { Id = 2, CustomerId = 1, InvoiceNumber = "H-CANCELLED", DueDate = new DateTime(2026, 9, 1), Balance = 3_000_000m, Status = InvoiceStatus.Cancelled }
            });

            // Act: Tính tuổi nợ
            var reports = (await _useCase.ExecuteAsync(DateTime.Now)).ToList();

            // Assert: Không tạo báo cáo nợ cho khách hàng này
            Assert.Empty(reports);
        }

        [Theory(DisplayName = "Domain Invoice: CalculateOverdueDays chỉ tính quá hạn khi Balance > 0 và asOfDate.Date > DueDate.Date")]
        [InlineData("2026-10-20", "2026-10-20", 1000000, 0, false)] // Đúng ngày hạn -> chưa quá hạn
        [InlineData("2026-10-20", "2026-10-21", 1000000, 0, false)] // Chưa đến hạn (ngày mai mới tới) -> 0 ngày
        [InlineData("2026-10-20", "2026-10-19", 1000000, 1, true)]  // Quá hạn 1 ngày
        [InlineData("2026-10-20", "2026-10-05", 1000000, 15, true)] // Quá hạn 15 ngày
        [InlineData("2026-10-20", "2026-10-05", 0, 0, false)]        // Đã thanh toán xong (Balance = 0) -> không quá hạn
        public void Invoice_CalculateOverdueDays_MatchesSingleSourceOfTruthRules(
            string asOfDateStr, 
            string dueDateStr, 
            decimal balance, 
            int expectedDays, 
            bool expectedIsOverdue)
        {
            var asOfDate = DateTime.Parse(asOfDateStr);
            var dueDate = DateTime.Parse(dueDateStr);

            int actualDays = Invoice.CalculateOverdueDays(dueDate, balance, asOfDate);
            bool actualIsOverdue = Invoice.CheckIsOverdue(dueDate, balance, asOfDate);

            Assert.Equal(expectedDays, actualDays);
            Assert.Equal(expectedIsOverdue, actualIsOverdue);
        }
    }
}
