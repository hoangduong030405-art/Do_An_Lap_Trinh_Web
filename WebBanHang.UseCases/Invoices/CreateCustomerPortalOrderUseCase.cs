using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class CreateCustomerPortalOrderUseCase : ICreateCustomerPortalOrderUseCase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IProductRepository _productRepository;

        public CreateCustomerPortalOrderUseCase(
            ICustomerRepository customerRepository,
            IInvoiceRepository invoiceRepository,
            IProductRepository productRepository)
        {
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
            _productRepository = productRepository;
        }

        public async Task<CustomerPortalOrderResultDto> ExecuteAsync(int customerId, List<InvoiceLine> items)
        {
            if (items == null || !items.Any())
            {
                return new CustomerPortalOrderResultDto
                {
                    Success = false,
                    ErrorMessage = "Giỏ hàng sỉ đang trống. Vui lòng chọn ít nhất một sản phẩm."
                };
            }

            if (items.Any(i => i.Quantity <= 0))
            {
                return new CustomerPortalOrderResultDto
                {
                    Success = false,
                    ErrorMessage = "Số lượng từng sản phẩm phải lớn hơn 0."
                };
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return new CustomerPortalOrderResultDto
                {
                    Success = false,
                    ErrorMessage = "Không tìm thấy hồ sơ Đại lý đối tác trong hệ thống."
                };
            }

            // Gộp các dòng cùng ProductId (GroupBy, cộng Quantity) trước khi kiểm tra tồn kho
            var mergedItems = items
                .GroupBy(i => i.ProductId)
                .Select(g => new InvoiceLine
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // 1. Kiểm tra tồn kho từng sản phẩm và gán giá chính thức từ IProductRepository
            // Bỏ qua hoàn toàn UnitPrice và CostPrice do client gửi lên nhằm bảo vệ an ninh giá
            foreach (var item in mergedItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return new CustomerPortalOrderResultDto
                    {
                        Success = false,
                        ErrorMessage = $"Không tìm thấy sản phẩm có mã ID #{item.ProductId} trong kho."
                    };
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new BusinessRuleException(
                        $"Sản phẩm '{product.Name}' không đủ tồn kho (Hiện có: {product.StockQuantity} {product.Unit}, Yêu cầu: {item.Quantity} {product.Unit}).");
                }

                // Ép buộc lấy đơn giá và giá vốn chính thức từ CSDL IProductRepository
                item.UnitPrice = product.UnitPrice;
                item.CostPrice = product.CostPrice;

                // Cập nhật lại giá cho các phần tử gốc trong items truyền vào
                foreach (var orig in items.Where(x => x.ProductId == item.ProductId))
                {
                    orig.UnitPrice = product.UnitPrice;
                    orig.CostPrice = product.CostPrice;
                }
            }

            // 2. Kiểm tra hạn mức nợ tín dụng thương mại (Luật 1)
            var currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
            var orderTotal = mergedItems.Sum(i => i.Quantity * i.UnitPrice);
            var availableCredit = Math.Max(0, customer.CreditLimit - currentDebt);

            if (currentDebt + orderTotal > customer.CreditLimit)
            {
                var exceededAmount = (currentDebt + orderTotal) - customer.CreditLimit;
                throw new CreditLimitExceededException(
                    $"Đơn hàng {orderTotal:N0} đ vượt hạn mức nợ còn lại (vượt {exceededAmount:N0} đ / Hạn mức khả dụng: {availableCredit:N0} đ). Quý khách vui lòng thanh toán bớt nợ cũ hoặc giảm bớt số lượng.");
            }

            // 3. Khởi tạo Hóa đơn B2B ghi nợ
            var now = DateTime.Now;
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-B2B-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                CustomerId = customerId,
                InvoiceDate = now,
                DueDate = customer.CalculateDueDate(now),
                TotalAmount = orderTotal,
                PaidAmount = 0,
                Balance = orderTotal,
                Status = InvoiceStatus.Unpaid
            };

            var invoiceId = await _invoiceRepository.CreateInvoiceWithLinesAsync(invoice, mergedItems);

            return new CustomerPortalOrderResultDto
            {
                Success = true,
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalAmount = orderTotal,
                RemainingAvailableCredit = Math.Max(0, customer.CreditLimit - (currentDebt + orderTotal))
            };
        }
    }
}
