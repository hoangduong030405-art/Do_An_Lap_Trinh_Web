using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.CoreBusiness.Exceptions;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.Payments
{
    public class RecordPaymentAndAllocateUseCase : IRecordPaymentAndAllocateUseCase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;

        public RecordPaymentAndAllocateUseCase(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<int> ExecuteAsync(int customerId, decimal paymentAmount, DateTime paymentDate, string? note)
        {
            if (paymentAmount <= 0)
            {
                throw new BusinessRuleException("Số tiền thanh toán phải lớn hơn 0.");
            }

            var rawInvoices = await _invoiceRepository.GetUnpaidInvoicesByCustomerOrderedByDateAsync(customerId);
            var unpaidInvoices = rawInvoices?
                .Where(i => i.Balance > 0 && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .OrderBy(i => i.InvoiceDate)
                .ThenBy(i => i.Id)
                .ToList() ?? new List<Invoice>();

            if (!unpaidInvoices.Any())
            {
                throw new BusinessRuleException("Khách hàng không có nợ cần thanh toán.");
            }
            
            var allocations = new List<PaymentAllocation>();
            var updatedInvoices = new List<Invoice>();
            
            decimal remainingAmount = paymentAmount;

            var payment = new Payment
            {
                CustomerId = customerId,
                PaymentDate = paymentDate,
                Amount = paymentAmount,
                Note = note ?? string.Empty
            };

            foreach (var invoice in unpaidInvoices)
            {
                if (remainingAmount <= 0) break;

                decimal allocateAmount = Math.Min(invoice.Balance, remainingAmount);
                
                invoice.PaidAmount += allocateAmount;
                invoice.Balance -= allocateAmount;
                invoice.Status = invoice.Balance <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
                
                remainingAmount -= allocateAmount;

                var allocation = new PaymentAllocation
                {
                    InvoiceId = invoice.Id,
                    AllocatedAmount = allocateAmount,
                    AllocationDate = paymentDate
                };

                allocations.Add(allocation);
                updatedInvoices.Add(invoice);
            }

            payment.UnallocatedAmount = remainingAmount;
            payment.PaymentNumber = $"PAY-{DateTime.Now:yyyyMMddHHmmss}";

            return await _paymentRepository.RecordPaymentWithAllocationsAsync(payment, allocations, updatedInvoices);
        }
    }
}
