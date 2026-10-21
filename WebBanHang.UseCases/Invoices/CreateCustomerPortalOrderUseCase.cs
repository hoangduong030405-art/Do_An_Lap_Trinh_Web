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

            // 1. Kiểm tra tồn kho từng sản phẩm
            foreach (var item in items)
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
            }

            // 2. Kiểm tra hạn mức nợ tín dụng thương mại (Luật 1)
            var currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
            var orderTotal = items.Sum(i => i.Quantity * i.UnitPrice);
            var availableCredit = Math.Max(0, customer.CreditLimit - currentDebt);

            if (currentDebt + orderTotal > customer.CreditLimit)
            {
                var exceededAmount = (currentDebt + orderTotal) - customer.CreditLimit;
                throw new CreditLimitExceededException(
                    $"Đơn hàng {orderTotal:N0} đ vượt hạn mức nợ còn lại (vượt {exceededAmount:N0} đ / Hạn mức khả dụng: {availableCredit:N0} đ). Quý khách vui lòng thanh toán bớt nợ cũ hoặc giảm bớt số lượng.");
            }

            // 3. Khởi tạo Hóa đơn B2B ghi nợ
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-B2B-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                CustomerId = customerId,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(customer.PaymentTermsDays > 0 ? customer.PaymentTermsDays : 30),
                TotalAmount = orderTotal,
                PaidAmount = 0,
                Balance = orderTotal,
                Status = InvoiceStatus.Unpaid
            };

            var invoiceId = await _invoiceRepository.CreateInvoiceWithLinesAsync(invoice, items);

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
