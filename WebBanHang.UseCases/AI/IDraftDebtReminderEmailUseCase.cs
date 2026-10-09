using System.Threading.Tasks;

namespace WebBanHang.UseCases.AI
{
    public interface IDraftDebtReminderEmailUseCase
    {
        Task<string> ExecuteAsync(int customerId);
    }
}
