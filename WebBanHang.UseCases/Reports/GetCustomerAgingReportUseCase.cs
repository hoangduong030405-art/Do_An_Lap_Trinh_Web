using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Reports
{
    public class GetCustomerAgingReportUseCase : IGetCustomerAgingReportUseCase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public GetCustomerAgingReportUseCase(ICustomerRepository customerRepository, IInvoiceRepository invoiceRepository)
        {
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<CustomerAgingReportDto>> ExecuteAsync(DateTime asOfDate)
        {
            var customers = await _customerRepository.GetAllAsync();
            var reports = new List<CustomerAgingReportDto>();

            foreach (var customer in customers)
            {
                var unpaidInvoices = await _invoiceRepository.GetUnpaidInvoicesByCustomerOrderedByDateAsync(customer.Id);
                if (!unpaidInvoices.Any()) continue;

                var report = new CustomerAgingReportDto
                {
                    CustomerId = customer.Id,
                    CustomerCode = customer.CustomerCode,
                    CustomerName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    CreditLimit = customer.CreditLimit
                };

                foreach (var invoice in unpaidInvoices)
                {
                    var overdueDays = (int)(asOfDate.Date - invoice.DueDate.Date).TotalDays;
                    if (overdueDays < 0) overdueDays = 0;

                    var invoiceDto = new InvoiceAgingItemDto
                    {
                        InvoiceId = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.InvoiceDate,
                        DueDate = invoice.DueDate,
                        TotalAmount = invoice.TotalAmount,
                        PaidAmount = invoice.PaidAmount,
                        Balance = invoice.Balance,
                        OverdueDays = overdueDays
                    };

                    if (overdueDays <= 30)
                    {
                        report.Bucket0To30 += invoice.Balance;
                        invoiceDto.AgingCategory = "0-30";
                    }
                    else if (overdueDays <= 60)
                    {
                        report.Bucket31To60 += invoice.Balance;
                        invoiceDto.AgingCategory = "31-60";
                    }
                    else if (overdueDays <= 90)
                    {
                        report.Bucket61To90 += invoice.Balance;
                        invoiceDto.AgingCategory = "61-90";
                    }
                    else
                    {
                        report.BucketOver90 += invoice.Balance;
                        invoiceDto.AgingCategory = ">90";
                    }

                    report.TotalOutstandingDebt += invoice.Balance;
                    report.Invoices.Add(invoiceDto);
                }

                reports.Add(report);
            }

            return reports.OrderByDescending(r => r.TotalOutstandingDebt);
        }
    }
}
