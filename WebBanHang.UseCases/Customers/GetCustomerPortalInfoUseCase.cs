using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Customers
{
    public class GetCustomerPortalInfoUseCase : IGetCustomerPortalInfoUseCase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public GetCustomerPortalInfoUseCase(
            ICustomerRepository customerRepository,
            IInvoiceRepository invoiceRepository)
        {
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<CustomerPortalInfoDto> ExecuteAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                return new CustomerPortalInfoDto { CustomerId = customerId };
            }

            var currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(customerId);
            var invoices = (await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId)).ToList();

            var unpaidInvoices = invoices.Where(i => i.Balance > 0 && i.Status != InvoiceStatus.Cancelled).ToList();
            var overdueInvoices = unpaidInvoices.Where(i => i.DueDate.Date < DateTime.Today).ToList();
            var overdueDebt = overdueInvoices.Sum(i => i.Balance);

            return new CustomerPortalInfoDto
            {
                CustomerId = customer.Id,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                CreditLimit = customer.CreditLimit,
                PaymentTermsDays = customer.PaymentTermsDays,
                CurrentDebt = currentDebt,
                TotalInvoicesCount = invoices.Count,
                UnpaidInvoicesCount = unpaidInvoices.Count,
                OverdueInvoicesCount = overdueInvoices.Count,
                OverdueDebtAmount = overdueDebt
            };
        }
    }
}
