using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Reports
{
    public interface IGetCustomerAgingReportUseCase
    {
        Task<IEnumerable<CustomerAgingReportDto>> ExecuteAsync(DateTime asOfDate);
    }
}
