using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class CreateInvoiceUseCase : ICreateInvoiceUseCase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IProductRepository _productRepository;

        public CreateInvoiceUseCase(
            ICustomerRepository customerRepository, 
            IInvoiceRepository invoiceRepository,
            IProductRepository productRepository)
        {
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
            _productRepository = productRepository;
        }

        public async Task<int> ExecuteAsync(int customerId, DateTime invoiceDate, List<InvoiceLine> lines)
        {
            if (lines == null || !lines.Any())
            {
                throw new BusinessRuleException("Danh sách sản phẩm không được rỗng.");
            }

            if (lines.Any(l => l.Quantity <= 0))
            {
                throw new BusinessRuleException("Số lượng từng sản phẩm phải lớn hơn 0.");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new BusinessRuleException("Không tìm thấy thông tin khách hàng.");
            }

            // Kiểm tra tồn kho từng sản phẩm
            foreach (var line in lines)
            {
                var product = await _productRepository.GetByIdAsync(line.ProductId);
                if (product == null)
                {
                    throw new BusinessRuleException($"Không tìm thấy sản phẩm có mã ID #{line.ProductId}.");
                }
                if (product.StockQuantity < line.Quantity)
                {
                    throw new BusinessRuleException(
                        $"Sản phẩm '{product.Name}' không đủ tồn kho (Hiện có: {product.StockQuantity}, Yêu cầu: {line.Quantity}).");
                }
            }

            // Kiểm tra hạn mức nợ
            var currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
            var newInvoiceAmount = lines.Sum(l => l.Quantity * l.UnitPrice);

            if (currentDebt + newInvoiceAmount > customer.CreditLimit)
            {
                throw new CreditLimitExceededException(
                    $"Không thể lập hóa đơn! Khách hàng '{customer.FullName}' đang nợ {currentDebt:N0} đ. Đơn mới {newInvoiceAmount:N0} đ khiến tổng nợ ({currentDebt + newInvoiceAmount:N0} đ) vượt quá hạn mức tín dụng cho phép ({customer.CreditLimit:N0} đ).");
            }

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                CustomerId = customerId,
                InvoiceDate = invoiceDate,
                DueDate = invoiceDate.AddDays(customer.PaymentTermsDays),
                TotalAmount = newInvoiceAmount,
                Balance = newInvoiceAmount,
                PaidAmount = 0,
                Status = InvoiceStatus.Unpaid
            };

            return await _invoiceRepository.CreateInvoiceWithLinesAsync(invoice, lines);
        }
    }
}
