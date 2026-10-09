using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.UseCases.Reports;

namespace WebBanHang.UseCases.AI
{
    public class DraftDebtReminderEmailUseCase : IDraftDebtReminderEmailUseCase
    {
        private readonly IGetCustomerAgingReportUseCase _getCustomerAgingReportUseCase;
        private readonly IAiDebtReminderService _aiDebtReminderService;

        public DraftDebtReminderEmailUseCase(
            IGetCustomerAgingReportUseCase getCustomerAgingReportUseCase, 
            IAiDebtReminderService aiDebtReminderService)
        {
            _getCustomerAgingReportUseCase = getCustomerAgingReportUseCase;
            _aiDebtReminderService = aiDebtReminderService;
        }

        public async Task<string> ExecuteAsync(int customerId)
        {
            var reports = await _getCustomerAgingReportUseCase.ExecuteAsync(DateTime.Now);
            var customerReport = reports.FirstOrDefault(r => r.CustomerId == customerId);
            
            if (customerReport == null || customerReport.TotalOutstandingDebt <= 0)
            {
                return "Khách hàng hiện không có nợ quá hạn.";
            }

            return await _aiDebtReminderService.GenerateReminderEmailAsync(customerReport);
        }
    }
}
