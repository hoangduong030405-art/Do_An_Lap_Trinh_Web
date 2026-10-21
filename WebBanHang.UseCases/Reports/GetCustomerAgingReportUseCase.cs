using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
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
                
                // Loại bỏ tuyệt đối hóa đơn đã thanh toán xong (Paid) hoặc bị hủy (Cancelled)
                var validInvoices = unpaidInvoices
                    .Where(i => i.Balance > 0 && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                    .ToList();

                if (!validInvoices.Any()) continue;

                var report = new CustomerAgingReportDto
                {
                    CustomerId = customer.Id,
                    CustomerCode = customer.CustomerCode,
                    CustomerName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    CreditLimit = customer.CreditLimit
                };

                foreach (var invoice in validInvoices)
                {
                    // Dùng hàm Domain duy nhất từ Invoice.cs
                    var overdueDays = invoice.GetOverdueDays(asOfDate);

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

                    if (overdueDays <= 0)
                    {
                        report.BucketNotDue += invoice.Balance;
                        invoiceDto.AgingCategory = "Chưa đến hạn";
                    }
                    else if (overdueDays <= 30)
                    {
                        report.Bucket1To30 += invoice.Balance;
                        invoiceDto.AgingCategory = "1-30";
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
