using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.UseCases.Invoices
{
    public interface ICreateInvoiceUseCase
    {
        Task<int> ExecuteAsync(int customerId, DateTime invoiceDate, List<InvoiceLine> lines);
    }
}
