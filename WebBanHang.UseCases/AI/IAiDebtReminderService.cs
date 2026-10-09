using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.AI
{
    public interface IAiDebtReminderService
    {
        Task<string> GenerateReminderEmailAsync(CustomerAgingReportDto agingReport);
    }
}
