using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.AI
{
    public interface IAiCreditRiskService
    {
        Task<CreditRiskAssessmentDto> AssessCustomerRiskAsync(Customer customer, CustomerAgingReportDto aging);
    }
}
