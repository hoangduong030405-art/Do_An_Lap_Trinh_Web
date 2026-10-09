using System.Collections.Generic;

namespace WebBanHang.UseCases.DTOs
{
    public class CreditRiskAssessmentDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal CurrentDebt { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal DebtUtilizationRate => CreditLimit > 0 ? (CurrentDebt / CreditLimit) * 100 : 100;
        public int MaxOverdueDays { get; set; }
        public int TotalUnpaidInvoices { get; set; }
        
        public int CreditScore { get; set; } // 0 - 100
        public string RatingGrade { get; set; } = "B"; // "A", "B", "C", "D"
        public string RiskLevel { get; set; } = "Thấp"; // "Rất thấp", "Trung bình", "Cao", "Rất cao"
        public string Recommendation { get; set; } = string.Empty;
        public List<string> RiskFactors { get; set; } = new();
    }
}
