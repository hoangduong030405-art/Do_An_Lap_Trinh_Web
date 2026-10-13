using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Invoices
{
    public class GetCustomerMyInvoicesUseCase : IGetCustomerMyInvoicesUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetCustomerMyInvoicesUseCase(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<CustomerPortalInvoiceDto>> ExecuteAsync(int customerId, string? filterStatus = null)
        {
            var invoices = await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId);

            var dtoList = invoices.Select(i => new CustomerPortalInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerId = i.CustomerId,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Balance = i.Balance,
                Status = i.Status
            });

            if (!string.IsNullOrWhiteSpace(filterStatus))
            {
                if (filterStatus == "Unpaid")
                {
                    dtoList = dtoList.Where(x => x.Balance > 0 && !x.IsOverdue);
                }
                else if (filterStatus == "Overdue")
                {
                    dtoList = dtoList.Where(x => x.IsOverdue);
                }
                else if (filterStatus == "Paid")
                {
                    dtoList = dtoList.Where(x => x.Balance <= 0);
                }
            }

            return dtoList;
        }
    }
}
