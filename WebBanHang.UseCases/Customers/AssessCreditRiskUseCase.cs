using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.UseCases.AI;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;
using WebBanHang.UseCases.Reports;

namespace WebBanHang.UseCases.Customers
{
    public class AssessCreditRiskUseCase : IAssessCreditRiskUseCase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IGetCustomerAgingReportUseCase _agingReportUseCase;
        private readonly IAiCreditRiskService _aiCreditRiskService;

        public AssessCreditRiskUseCase(
            ICustomerRepository customerRepository,
            IGetCustomerAgingReportUseCase agingReportUseCase,
            IAiCreditRiskService aiCreditRiskService)
        {
            _customerRepository = customerRepository;
            _agingReportUseCase = agingReportUseCase;
            _aiCreditRiskService = aiCreditRiskService;
        }

        public async Task<CreditRiskAssessmentDto> ExecuteAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            var agings = await _agingReportUseCase.ExecuteAsync(DateTime.Now);
            var aging = agings.FirstOrDefault(a => a.CustomerId == customerId) ?? new CustomerAgingReportDto
            {
                CustomerId = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.FullName
            };

            return await _aiCreditRiskService.AssessCustomerRiskAsync(customer, aging);
        }

        public async Task<IEnumerable<CreditRiskAssessmentDto>> ExecuteAllAsync()
        {
            var customers = (await _customerRepository.GetAllAsync()).ToList();
            var agings = (await _agingReportUseCase.ExecuteAsync(DateTime.Now)).ToDictionary(a => a.CustomerId);

            var list = new List<CreditRiskAssessmentDto>();
            foreach (var c in customers)
            {
                agings.TryGetValue(c.Id, out var aging);
                aging ??= new CustomerAgingReportDto { CustomerId = c.Id, CustomerCode = c.CustomerCode, CustomerName = c.FullName };
                var assessment = await _aiCreditRiskService.AssessCustomerRiskAsync(c, aging);
                list.Add(assessment);
            }

            return list;
        }
    }
}
