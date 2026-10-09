using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.Customers
{
    public interface IAssessCreditRiskUseCase
    {
        Task<CreditRiskAssessmentDto> ExecuteAsync(int customerId);
        Task<IEnumerable<CreditRiskAssessmentDto>> ExecuteAllAsync();
    }
}
