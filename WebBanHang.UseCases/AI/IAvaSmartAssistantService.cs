using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.AI
{
    public interface IAvaSmartAssistantService
    {
        Task<IEnumerable<SmartReminderDto>> GetSmartRemindersAsync(string role, int? customerId = null);
        Task<AvaAiAnswerDto> AnswerAccountingQuestionAsync(string question, string role, int? customerId = null);
    }

    public interface IGetSmartRemindersUseCase
    {
        Task<IEnumerable<SmartReminderDto>> ExecuteAsync(string role, int? customerId = null);
    }

    public class GetSmartRemindersUseCase : IGetSmartRemindersUseCase
    {
        private readonly IAvaSmartAssistantService _assistantService;

        public GetSmartRemindersUseCase(IAvaSmartAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        public async Task<IEnumerable<SmartReminderDto>> ExecuteAsync(string role, int? customerId = null)
        {
            return await _assistantService.GetSmartRemindersAsync(role, customerId);
        }
    }

    public interface IAnswerAccountingQuestionUseCase
    {
        Task<AvaAiAnswerDto> ExecuteAsync(string question, string role, int? customerId = null);
    }

    public class AnswerAccountingQuestionUseCase : IAnswerAccountingQuestionUseCase
    {
        private readonly IAvaSmartAssistantService _assistantService;

        public AnswerAccountingQuestionUseCase(IAvaSmartAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        public async Task<AvaAiAnswerDto> ExecuteAsync(string question, string role, int? customerId = null)
        {
            return await _assistantService.AnswerAccountingQuestionAsync(question, role, customerId);
        }
    }
}
