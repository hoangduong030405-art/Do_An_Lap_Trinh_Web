using System;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class ViewInvoiceDetailUseCase : IViewInvoiceDetailUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public ViewInvoiceDetailUseCase(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceDetailDto?> ExecuteAsync(int id, string? role = null, int? customerId = null)
        {
            var invoice = await _invoiceRepository.GetInvoiceDetailByIdAsync(id);
            if (invoice == null) return null;

            // Kiểm tra phân quyền bảo vệ tài nguyên:
            // Nếu người dùng thuộc vai trò Customer thì chỉ cho xem hóa đơn có CustomerId == claim CustomerId
            if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                if (!customerId.HasValue || invoice.CustomerId != customerId.Value)
                {
                    return null;
                }
            }

            return invoice;
        }
    }
}
