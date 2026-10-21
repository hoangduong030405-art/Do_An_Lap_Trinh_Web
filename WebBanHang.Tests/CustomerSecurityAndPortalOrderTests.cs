using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.Tests.Fakes;
using WebBanHang.UseCases.AI;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.Invoices;
using Xunit;

namespace WebBanHang.Tests
{
    public class CustomerSecurityAndPortalOrderTests
    {
        private readonly FakeCustomerRepository _customerRepo = new();
        private readonly FakeInvoiceRepository _invoiceRepo = new();
        private readonly FakeProductRepository _productRepo = new();

        [Fact(DisplayName = "Bảo mật: Customer xem hóa đơn của khách hàng khác bị chặn (trả về null)")]
        public async Task ViewInvoiceDetail_WhenCustomerViewsAnotherCustomersInvoice_IsBlockedAndReturnsNull()
        {
            // Arrange: Hóa đơn #1 thuộc sở hữu của Khách hàng 2 (Đại Lý Hùng Thắng)
            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "INV-001",
                CustomerId = 2,
                InvoiceDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(20),
                TotalAmount = 5_000_000m,
                PaidAmount = 0m,
                Balance = 5_000_000m,
                Status = InvoiceStatus.Unpaid
            };
            _invoiceRepo.Invoices.Add(invoice);

            var useCase = new ViewInvoiceDetailUseCase(_invoiceRepo);

            // Act 1: Khách hàng 1 (Tạp Hóa Bà Ba) cố tình truy cập xem hóa đơn #1 của Khách hàng 2
            var resultCustomer1 = await useCase.ExecuteAsync(1, role: "Customer", customerId: 1);

            // Act 2: Khách hàng 2 (chính chủ) truy cập hóa đơn #1
            var resultOwner = await useCase.ExecuteAsync(1, role: "Customer", customerId: 2);

            // Act 3: Kế toán (Accountant) hoặc Bán hàng (Sales) truy cập hóa đơn #1
            var resultAccountant = await useCase.ExecuteAsync(1, role: "Accountant", customerId: null);

            // Assert
            Assert.Null(resultCustomer1); // Bị chặn hoàn toàn (BOLA/IDOR prevention)
            Assert.NotNull(resultOwner); // Chính chủ xem được
            Assert.Equal(2, resultOwner.CustomerId);
            Assert.NotNull(resultAccountant); // Kế toán xem được
            Assert.Equal(2, resultAccountant.CustomerId);
        }

        [Fact(DisplayName = "Bảo mật: Customer hỏi AVA về nợ quá hạn không thấy thông tin và tên khách hàng khác")]
        public async Task AvaAssistant_WhenCustomerAsksAboutOverdueDebts_DoesNotExposeOtherCustomersData()
        {
            // Arrange:
            // Khách hàng 1: Tạp Hóa Bà Ba (không có nợ quá hạn)
            var customer1 = new Customer
            {
                Id = 1,
                CustomerCode = "KH001",
                FullName = "Tạp Hóa Bà Ba",
                CreditLimit = 10_000_000m,
                PaymentTermsDays = 30
            };
            // Khách hàng 2: Đại Lý Hùng Thắng (đang nợ quá hạn 35 triệu)
            var customer2 = new Customer
            {
                Id = 2,
                CustomerCode = "KH002",
                FullName = "Đại Lý Hùng Thắng",
                CreditLimit = 50_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer1);
            _customerRepo.Customers.Add(customer2);

            // Hóa đơn của Khách 2 quá hạn 15 ngày
            _invoiceRepo.CustomInvoicesWithCustomer.Add(new InvoiceListDto
            {
                Id = 201,
                InvoiceNumber = "INV-OVERDUE-002",
                CustomerId = 2,
                CustomerName = "Đại Lý Hùng Thắng",
                CustomerCode = "KH002",
                InvoiceDate = DateTime.Now.AddDays(-45),
                DueDate = DateTime.Now.AddDays(-15),
                TotalAmount = 35_000_000m,
                PaidAmount = 0m,
                Balance = 35_000_000m,
                Status = InvoiceStatus.Unpaid
            });

            // Hóa đơn của Khách 1 trong hạn
            _invoiceRepo.CustomInvoicesWithCustomer.Add(new InvoiceListDto
            {
                Id = 101,
                InvoiceNumber = "INV-OK-001",
                CustomerId = 1,
                CustomerName = "Tạp Hóa Bà Ba",
                CustomerCode = "KH001",
                InvoiceDate = DateTime.Now.AddDays(-5),
                DueDate = DateTime.Now.AddDays(25),
                TotalAmount = 4_000_000m,
                PaidAmount = 0m,
                Balance = 4_000_000m,
                Status = InvoiceStatus.Unpaid
            });

            var avaService = new AvaSmartAssistantService(_invoiceRepo, _customerRepo, _productRepo);

            // Act: Khách hàng 1 hỏi AVA: "Ai đang nợ quá hạn?"
            var customerAnswer = await avaService.AnswerAccountingQuestionAsync("Ai đang nợ quá hạn?", role: "Customer", customerId: 1);

            // Kế toán hỏi AVA: "Ai đang nợ quá hạn?"
            var accountantAnswer = await avaService.AnswerAccountingQuestionAsync("Ai đang nợ quá hạn?", role: "Accountant", customerId: null);

            // Assert
            // 1. Phản hồi cho Khách hàng 1: TUYỆT ĐỐI không chứa tên hoặc số nợ của Khách hàng 2
            Assert.DoesNotContain("Hùng Thắng", customerAnswer.Answer);
            Assert.DoesNotContain("35,000,000", customerAnswer.Answer);
            Assert.DoesNotContain("35.000.000", customerAnswer.Answer);
            Assert.DoesNotContain("INV-OVERDUE-002", customerAnswer.Answer);
            Assert.Contains("không có hóa đơn nào quá hạn", customerAnswer.Answer);

            // 2. Phản hồi cho Kế toán: Hiển thị đầy đủ thông tin nợ xấu của Khách 2
            Assert.Contains("Hùng Thắng", accountantAnswer.Answer);
            var expectedDebtFormatted = 35_000_000m.ToString("N0");
            Assert.Contains(expectedDebtFormatted, accountantAnswer.Answer);
        }

        [Fact(DisplayName = "Nghiệp vụ B2B Portal: Đơn đặt hàng với UnitPrice = 0 vẫn bị ghi đè và tính theo giá gốc từ IProductRepository")]
        public async Task CreateCustomerPortalOrder_WhenClientSendsUnitPriceZero_CalculatesTotalUsingRepositoryPrice()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                CustomerCode = "KH001",
                FullName = "Tạp Hóa Bà Ba",
                CreditLimit = 20_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[1] = 0m;

            var product = new Product
            {
                Id = 501,
                ProductCode = "SP501",
                Name = "Thùng Nước Tăng Lực RedBull",
                Unit = "Thùng",
                UnitPrice = 300_000m, // Giá niêm yết trong kho CSDL
                CostPrice = 240_000m, // Giá vốn trong kho CSDL
                StockQuantity = 50
            };
            _productRepo.Products.Add(product);

            var portalUseCase = new CreateCustomerPortalOrderUseCase(_customerRepo, _invoiceRepo, _productRepo);

            // Khách hàng gian lận cố tình gửi UnitPrice = 0 và CostPrice = 0
            var maliciousItems = new List<InvoiceLine>
            {
                new InvoiceLine
                {
                    ProductId = 501,
                    Quantity = 10,
                    UnitPrice = 0m,
                    CostPrice = 0m
                }
            };

            // Act
            var result = await portalUseCase.ExecuteAsync(1, maliciousItems);

            // Assert
            Assert.True(result.Success);
            // Tổng tiền đơn hàng phải được tính theo giá gốc 300.000 * 10 = 3.000.000 đ chứ không phải 0 đ
            Assert.Equal(3_000_000m, result.TotalAmount);
            // UnitPrice và CostPrice trong dòng chi tiết phải được cập nhật lại theo CSDL
            Assert.Equal(300_000m, maliciousItems[0].UnitPrice);
            Assert.Equal(240_000m, maliciousItems[0].CostPrice);

            // Hóa đơn được lưu vào repository phải có TotalAmount = 3.000.000 đ
            var savedInvoice = _invoiceRepo.Invoices.FirstOrDefault(i => i.Id == result.InvoiceId);
            Assert.NotNull(savedInvoice);
            Assert.Equal(3_000_000m, savedInvoice.TotalAmount);
            Assert.Equal(3_000_000m, savedInvoice.Balance);
        }

        [Theory(DisplayName = "Khắc phục lỗi Dapper: Chuỗi trạng thái 'Pending' từ CSDL SQL Server ánh xạ chuẩn xác sang InvoiceStatus.Unpaid")]
        [InlineData("Pending", InvoiceStatus.Unpaid)]
        [InlineData("Unpaid", InvoiceStatus.Unpaid)]
        [InlineData("PartiallyPaid", InvoiceStatus.PartiallyPaid)]
        [InlineData("Paid", InvoiceStatus.Paid)]
        [InlineData("Cancelled", InvoiceStatus.Cancelled)]
        public void InvoiceStatus_CanParseFromDatabaseValues(string dbValue, InvoiceStatus expected)
        {
            var parsed = Enum.Parse<InvoiceStatus>(dbValue, ignoreCase: true);
            Assert.Equal(expected, parsed);
        }
    }
}
