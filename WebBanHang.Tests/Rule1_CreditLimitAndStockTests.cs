using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.Tests.Fakes;
using WebBanHang.UseCases.Invoices;
using Xunit;

namespace WebBanHang.Tests
{
    public class Rule1_CreditLimitAndStockTests
    {
        private readonly FakeCustomerRepository _customerRepo = new();
        private readonly FakeInvoiceRepository _invoiceRepo = new();
        private readonly FakeProductRepository _productRepo = new();
        private readonly CreateInvoiceUseCase _useCase;

        public Rule1_CreditLimitAndStockTests()
        {
            _useCase = new CreateInvoiceUseCase(_customerRepo, _invoiceRepo, _productRepo);
        }

        [Fact(DisplayName = "Luật 1(a): Khi Dư nợ hiện tại + Đơn hàng mới > Hạn mức tín dụng thì ném CreditLimitExceededException")]
        public async Task CreateInvoice_WhenDebtPlusNewAmountExceedsCreditLimit_ThrowsCreditLimitExceededException()
        {
            // Arrange: Khách hàng có hạn mức 10 triệu, hiện đang nợ 6 triệu
            var customer = new Customer
            {
                Id = 1,
                CustomerCode = "KH001",
                FullName = "Tạp Hóa Bà Ba",
                CreditLimit = 10_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[1] = 6_000_000m;

            var product = new Product
            {
                Id = 101,
                ProductCode = "SP001",
                Name = "Coca-Cola Sleek",
                UnitPrice = 200_000m,
                StockQuantity = 100
            };
            _productRepo.Products.Add(product);

            // Đơn hàng mới: 25 thùng * 200.000 = 5.000.000 đ
            // Tổng nợ dự kiến = 6.000.000 + 5.000.000 = 11.000.000 đ > Hạn mức 10.000.000 đ
            var lines = new List<InvoiceLine>
            {
                new() { ProductId = 101, Quantity = 25, UnitPrice = 200_000m }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CreditLimitExceededException>(() =>
                _useCase.ExecuteAsync(customer.Id, DateTime.Now, lines));

            Assert.Contains("vượt quá hạn mức tín dụng", ex.Message);
            Assert.Empty(_invoiceRepo.Invoices); // Không được tạo hóa đơn
        }

        [Fact(DisplayName = "Luật 1(b): Khi Dư nợ hiện tại + Đơn hàng mới == Đúng bằng hạn mức tín dụng thì cho phép lập hóa đơn")]
        public async Task CreateInvoice_WhenDebtPlusNewAmountEqualsExactCreditLimit_AllowsCreationSuccessfully()
        {
            // Arrange: Khách hàng có hạn mức 10 triệu, hiện đang nợ 6 triệu
            var customer = new Customer
            {
                Id = 1,
                CustomerCode = "KH001",
                FullName = "Tạp Hóa Bà Ba",
                CreditLimit = 10_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[1] = 6_000_000m;

            var product = new Product
            {
                Id = 101,
                ProductCode = "SP001",
                Name = "Coca-Cola Sleek",
                UnitPrice = 200_000m,
                StockQuantity = 100
            };
            _productRepo.Products.Add(product);

            // Đơn hàng mới: 20 thùng * 200.000 = 4.000.000 đ
            // Tổng nợ dự kiến = 6.000.000 + 4.000.000 = 10.000.000 đ == Đúng hạn mức 10.000.000 đ
            var lines = new List<InvoiceLine>
            {
                new() { ProductId = 101, Quantity = 20, UnitPrice = 200_000m }
            };

            // Act
            var invoiceId = await _useCase.ExecuteAsync(customer.Id, DateTime.Now, lines);

            // Assert
            Assert.True(invoiceId > 0);
            Assert.Single(_invoiceRepo.Invoices);
            var created = _invoiceRepo.Invoices[0];
            Assert.Equal(4_000_000m, created.TotalAmount);
            Assert.Equal(4_000_000m, created.Balance);
            Assert.Equal(InvoiceStatus.Unpaid, created.Status);
        }

        [Fact(DisplayName = "Luật 1(c): Khi Số lượng đặt mua > Số lượng tồn kho hiện có thì ném BusinessRuleException")]
        public async Task CreateInvoice_WhenQuantityExceedsAvailableStock_ThrowsBusinessRuleException()
        {
            // Arrange: Sản phẩm chỉ còn tồn kho 15 cái
            var customer = new Customer
            {
                Id = 2,
                CustomerCode = "KH002",
                FullName = "Siêu Thị Mini Tuấn Hương",
                CreditLimit = 50_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[2] = 0m;

            var product = new Product
            {
                Id = 102,
                ProductCode = "SP002",
                Name = "Heineken Silver",
                UnitPrice = 435_000m,
                StockQuantity = 15 // Tồn kho chỉ còn 15
            };
            _productRepo.Products.Add(product);

            // Đặt mua 20 thùng > 15 thùng tồn kho
            var lines = new List<InvoiceLine>
            {
                new() { ProductId = 102, Quantity = 20, UnitPrice = 435_000m }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _useCase.ExecuteAsync(customer.Id, DateTime.Now, lines));

            Assert.Contains("không đủ tồn kho", ex.Message);
            Assert.Empty(_invoiceRepo.Invoices); // Không được tạo hóa đơn
        }

        [Fact(DisplayName = "Luật 1 Portal: Khi Khách hàng đặt qua Portal vượt Hạn mức tín dụng thì ném CreditLimitExceededException")]
        public async Task CreateCustomerPortalOrder_WhenDebtPlusNewAmountExceedsCreditLimit_ThrowsCreditLimitExceededException()
        {
            var portalUseCase = new CreateCustomerPortalOrderUseCase(_customerRepo, _invoiceRepo, _productRepo);

            var customer = new Customer
            {
                Id = 10,
                CustomerCode = "KH010",
                FullName = "Đại Lý Phương Nam",
                CreditLimit = 20_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[10] = 15_000_000m; // Đang nợ 15 triệu

            var product = new Product
            {
                Id = 201,
                ProductCode = "SP201",
                Name = "Tiger Crystal",
                UnitPrice = 400_000m,
                StockQuantity = 100
            };
            _productRepo.Products.Add(product);

            // Đơn hàng: 15 thùng * 400.000 = 6.000.000 đ
            // Tổng nợ = 15.000.000 + 6.000.000 = 21.000.000 đ > Hạn mức 20.000.000 đ
            var items = new List<InvoiceLine>
            {
                new() { ProductId = 201, Quantity = 15, UnitPrice = 400_000m }
            };

            var ex = await Assert.ThrowsAsync<CreditLimitExceededException>(() =>
                portalUseCase.ExecuteAsync(customer.Id, items));

            Assert.Contains("vượt hạn mức nợ", ex.Message);
        }

        [Fact(DisplayName = "Luật 1 Portal: Khi Khách hàng đặt qua Portal vượt Tồn kho hiện có thì ném BusinessRuleException")]
        public async Task CreateCustomerPortalOrder_WhenQuantityExceedsAvailableStock_ThrowsBusinessRuleException()
        {
            var portalUseCase = new CreateCustomerPortalOrderUseCase(_customerRepo, _invoiceRepo, _productRepo);

            var customer = new Customer
            {
                Id = 11,
                CustomerCode = "KH011",
                FullName = "Đại Lý Miền Trung",
                CreditLimit = 50_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[11] = 0m;

            var product = new Product
            {
                Id = 202,
                ProductCode = "SP202",
                Name = "Mì Hảo Hảo",
                UnitPrice = 125_000m,
                StockQuantity = 8 // Chỉ còn 8 thùng
            };
            _productRepo.Products.Add(product);

            // Đặt 10 thùng > 8 thùng tồn kho
            var items = new List<InvoiceLine>
            {
                new() { ProductId = 202, Quantity = 10, UnitPrice = 125_000m }
            };

            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                portalUseCase.ExecuteAsync(customer.Id, items));

            Assert.Contains("không đủ tồn kho", ex.Message);
        }

        [Fact(DisplayName = "Luật 1: Gộp dòng cùng ProductId - Khi tổng số lượng các dòng trùng sản phẩm vượt tồn kho thì ném BusinessRuleException")]
        public async Task CreateInvoice_WhenMultipleLinesForSameProductExceedStock_ThrowsBusinessRuleException()
        {
            // Arrange: Tồn kho chỉ còn 15
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
                Id = 301,
                ProductCode = "SP301",
                Name = "Nước Tăng Lực Red Bull",
                UnitPrice = 250_000m,
                StockQuantity = 15 // Tồn kho 15
            };
            _productRepo.Products.Add(product);

            // Gửi 2 dòng cùng ProductId 301: Dòng 1 (10) + Dòng 2 (10) = 20 > 15
            // Nếu không gộp, từng dòng 10 <= 15 sẽ lọt qua kiểm tra riêng lẻ!
            var lines = new List<InvoiceLine>
            {
                new() { ProductId = 301, Quantity = 10, UnitPrice = 250_000m },
                new() { ProductId = 301, Quantity = 10, UnitPrice = 250_000m }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _useCase.ExecuteAsync(customer.Id, DateTime.Now, lines));

            Assert.Contains("không đủ tồn kho", ex.Message);
            Assert.Contains("Yêu cầu: 20", ex.Message);
        }

        [Fact(DisplayName = "Luật 1: Gộp dòng cùng ProductId - Khi tổng số lượng đủ tồn kho thì hợp nhất thành 1 dòng chi tiết trong hóa đơn")]
        public async Task CreateInvoice_WhenMultipleLinesForSameProductWithinStock_MergesIntoSingleInvoiceLine()
        {
            // Arrange: Tồn kho 30
            var customer = new Customer
            {
                Id = 2,
                CustomerCode = "KH002",
                FullName = "Đại Lý Hùng Thắng",
                CreditLimit = 20_000_000m,
                PaymentTermsDays = 30
            };
            _customerRepo.Customers.Add(customer);
            _customerRepo.OutstandingDebts[2] = 0m;

            var product = new Product
            {
                Id = 302,
                ProductCode = "SP302",
                Name = "Sữa Tươi Vinamilk",
                UnitPrice = 350_000m,
                StockQuantity = 30
            };
            _productRepo.Products.Add(product);

            // Gửi 2 dòng cùng ProductId 302: Dòng 1 (5) + Dòng 2 (8) = 13 <= 30
            var lines = new List<InvoiceLine>
            {
                new() { ProductId = 302, Quantity = 5, UnitPrice = 350_000m },
                new() { ProductId = 302, Quantity = 8, UnitPrice = 350_000m }
            };

            // Act
            var invoiceId = await _useCase.ExecuteAsync(customer.Id, DateTime.Now, lines);

            // Assert
            Assert.True(invoiceId > 0);
            var savedLines = _invoiceRepo.InvoiceLines.Where(l => l.InvoiceId == invoiceId).ToList();
            // Đã được gộp thành đúng 1 dòng với số lượng 13
            Assert.Single(savedLines);
            Assert.Equal(13, savedLines[0].Quantity);
            Assert.Equal(13 * 350_000m, _invoiceRepo.Invoices.First(i => i.Id == invoiceId).TotalAmount);
        }
    }
}
