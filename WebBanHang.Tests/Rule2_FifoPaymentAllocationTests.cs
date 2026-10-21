using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.Tests.Fakes;
using WebBanHang.UseCases.Payments;
using Xunit;

namespace WebBanHang.Tests
{
    public class Rule2_FifoPaymentAllocationTests
    {
        private readonly FakeInvoiceRepository _invoiceRepo = new();
        private readonly FakePaymentRepository _paymentRepo = new();
        private readonly RecordPaymentAndAllocateUseCase _useCase;

        public Rule2_FifoPaymentAllocationTests()
        {
            _useCase = new RecordPaymentAndAllocateUseCase(_invoiceRepo, _paymentRepo);
        }

        [Fact(DisplayName = "Luật 2(d): Tiền thu 5tr, HĐ cũ 3tr + HĐ mới 4tr => Phân bổ 3tr cho HĐ cũ trước, 2tr cho HĐ mới sau đúng thứ tự InvoiceDate (FIFO)")]
        public async Task RecordPayment_WhenAmount5M_AllocatesToOldestInvoice3M_ThenNextInvoice2M_InFifoDateOrder()
        {
            // Arrange: Khách hàng 1 có 2 hóa đơn còn nợ
            var oldInvoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "HD-CU",
                CustomerId = 1,
                InvoiceDate = new DateTime(2026, 9, 1),
                DueDate = new DateTime(2026, 10, 1),
                TotalAmount = 3_000_000m,
                Balance = 3_000_000m,
                PaidAmount = 0,
                Status = InvoiceStatus.Unpaid
            };

            var newInvoice = new Invoice
            {
                Id = 2,
                InvoiceNumber = "HD-MOI",
                CustomerId = 1,
                InvoiceDate = new DateTime(2026, 10, 1),
                DueDate = new DateTime(2026, 11, 1),
                TotalAmount = 4_000_000m,
                Balance = 4_000_000m,
                PaidAmount = 0,
                Status = InvoiceStatus.Unpaid
            };

            // CỐ TÌNH nạp hóa đơn mới trước, hóa đơn cũ sau vào Fake để Fake trả về thứ tự NGƯỢC (mới trước, cũ sau)
            // Fake không tự OrderBy. UseCase bắt buộc phải tự sắp xếp OrderBy(i => i.InvoiceDate) thì mới phân bổ đúng FIFO!
            _invoiceRepo.Invoices.Add(newInvoice);
            _invoiceRepo.Invoices.Add(oldInvoice);

            // Act: Khách trả 5.000.000 đ
            var paymentId = await _useCase.ExecuteAsync(1, 5_000_000m, DateTime.Now, "Thu nợ đợt 1");

            // Assert
            Assert.True(paymentId > 0);
            Assert.Equal(2, _paymentRepo.Allocations.Count);

            // Phân bổ 1: HĐ cũ được thanh toán trọn 3tr trước
            var alloc1 = _paymentRepo.Allocations[0];
            Assert.Equal(oldInvoice.Id, alloc1.InvoiceId);
            Assert.Equal(3_000_000m, alloc1.AllocatedAmount);
            Assert.Equal(0m, oldInvoice.Balance);
            Assert.Equal(3_000_000m, oldInvoice.PaidAmount);
            Assert.Equal(InvoiceStatus.Paid, oldInvoice.Status);

            // Phân bổ 2: HĐ mới được thanh toán 2tr còn lại
            var alloc2 = _paymentRepo.Allocations[1];
            Assert.Equal(newInvoice.Id, alloc2.InvoiceId);
            Assert.Equal(2_000_000m, alloc2.AllocatedAmount);
            Assert.Equal(2_000_000m, newInvoice.Balance);
            Assert.Equal(2_000_000m, newInvoice.PaidAmount);
            Assert.Equal(InvoiceStatus.PartiallyPaid, newInvoice.Status);
        }

        [Fact(DisplayName = "Luật 2(e): Tổng số tiền phân bổ (Σ Phân bổ) luôn luôn <= Số tiền phiếu thu thực tế")]
        public async Task RecordPayment_TotalAllocatedAmount_IsAlwaysLessThanOrEqualToPaymentAmount()
        {
            // Arrange: 3 hóa đơn tổng nợ 10 triệu
            _invoiceRepo.Invoices.Add(new Invoice { Id = 1, CustomerId = 1, InvoiceDate = new DateTime(2026, 8, 1), Balance = 3_000_000m, Status = InvoiceStatus.Unpaid });
            _invoiceRepo.Invoices.Add(new Invoice { Id = 2, CustomerId = 1, InvoiceDate = new DateTime(2026, 9, 1), Balance = 4_000_000m, Status = InvoiceStatus.Unpaid });
            _invoiceRepo.Invoices.Add(new Invoice { Id = 3, CustomerId = 1, InvoiceDate = new DateTime(2026, 10, 1), Balance = 3_000_000m, Status = InvoiceStatus.Unpaid });

            decimal paymentAmount = 6_500_000m;

            // Act
            await _useCase.ExecuteAsync(1, paymentAmount, DateTime.Now, "Thu tiền một phần");

            // Assert: Tổng phân bổ phải bằng 6.500.000 đ <= 6.500.000 đ
            decimal totalAllocated = _paymentRepo.Allocations.Sum(a => a.AllocatedAmount);
            Assert.True(totalAllocated <= paymentAmount);
            Assert.Equal(paymentAmount, totalAllocated);
        }

        [Fact(DisplayName = "Luật 2(f): Khi Số tiền thu > Tổng nợ khách hàng, phần tiền dư thừa được ghi nhận chuẩn xác vào UnallocatedAmount")]
        public async Task RecordPayment_WhenPaymentExceedsTotalDebt_StoresRemainingAmountInUnallocatedAmount()
        {
            // Arrange: Khách hàng nợ duy nhất 1 hóa đơn 3 triệu
            _invoiceRepo.Invoices.Add(new Invoice
            {
                Id = 1,
                CustomerId = 1,
                InvoiceDate = new DateTime(2026, 9, 1),
                TotalAmount = 3_000_000m,
                Balance = 3_000_000m,
                PaidAmount = 0,
                Status = InvoiceStatus.Unpaid
            });

            // Act: Khách thanh toán 5 triệu (dư 2 triệu)
            await _useCase.ExecuteAsync(1, 5_000_000m, DateTime.Now, "Khách trả thừa 2tr");

            // Assert
            var payment = _paymentRepo.Payments.Single();
            Assert.Equal(5_000_000m, payment.Amount);
            Assert.Equal(2_000_000m, payment.UnallocatedAmount); // Tiền dư chưa phân bổ

            var alloc = _paymentRepo.Allocations.Single();
            Assert.Equal(3_000_000m, alloc.AllocatedAmount);
        }

        [Fact(DisplayName = "Luật 2(g1): Khi Khách hàng không có hóa đơn nợ nào thì ném lỗi thân thiện BusinessRuleException")]
        public async Task RecordPayment_WhenCustomerHasNoDebt_ThrowsFriendlyBusinessRuleException()
        {
            // Arrange: Không có hóa đơn nào của khách 1 trong repository
            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _useCase.ExecuteAsync(1, 1_000_000m, DateTime.Now, "Thu tiền khi không có nợ"));

            Assert.Contains("không có nợ", ex.Message);
            Assert.Empty(_paymentRepo.Payments);
        }

        [Theory(DisplayName = "Luật 2(g2): Khi Số tiền thanh toán <= 0 thì ném lỗi thân thiện BusinessRuleException")]
        [InlineData(0)]
        [InlineData(-500000)]
        public async Task RecordPayment_WhenPaymentAmountIsZeroOrNegative_ThrowsFriendlyBusinessRuleException(decimal invalidAmount)
        {
            // Arrange: Có hóa đơn nợ
            _invoiceRepo.Invoices.Add(new Invoice { Id = 1, CustomerId = 1, InvoiceDate = DateTime.Now, Balance = 3_000_000m, Status = InvoiceStatus.Unpaid });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _useCase.ExecuteAsync(1, invalidAmount, DateTime.Now, "Số tiền không hợp lệ"));

            Assert.Contains("lớn hơn 0", ex.Message);
            Assert.Empty(_paymentRepo.Payments);
        }

        [Fact(DisplayName = "Luật 2(h): Hóa đơn được thanh toán đủ chuyển Status sang Paid, hóa đơn thanh toán một phần chuyển Status sang PartiallyPaid")]
        public async Task RecordPayment_InvoiceFullyPaidBecomesPaid_PartiallyPaidBecomesPartiallyPaid()
        {
            // Arrange: HĐ 1 (3tr), HĐ 2 (4tr)
            var inv1 = new Invoice { Id = 1, CustomerId = 1, InvoiceDate = new DateTime(2026, 9, 1), Balance = 3_000_000m, Status = InvoiceStatus.Unpaid };
            var inv2 = new Invoice { Id = 2, CustomerId = 1, InvoiceDate = new DateTime(2026, 10, 1), Balance = 4_000_000m, Status = InvoiceStatus.Unpaid };
            _invoiceRepo.Invoices.Add(inv1);
            _invoiceRepo.Invoices.Add(inv2);

            // Act: Thu 5 triệu -> HĐ 1 trả đủ 3tr, HĐ 2 trả 2tr (còn nợ 2tr)
            await _useCase.ExecuteAsync(1, 5_000_000m, DateTime.Now, "Thu tiền");

            // Assert
            Assert.Equal(InvoiceStatus.Paid, inv1.Status);
            Assert.Equal(0m, inv1.Balance);

            Assert.Equal(InvoiceStatus.PartiallyPaid, inv2.Status);
            Assert.Equal(2_000_000m, inv2.Balance);
        }
    }
}
