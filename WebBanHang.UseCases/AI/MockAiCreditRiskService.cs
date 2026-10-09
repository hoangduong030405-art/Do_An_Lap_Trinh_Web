using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.AI
{
    public class MockAiCreditRiskService : IAiCreditRiskService
    {
        public Task<CreditRiskAssessmentDto> AssessCustomerRiskAsync(Customer customer, CustomerAgingReportDto aging)
        {
            var maxOverdue = aging.Invoices.Any() ? aging.Invoices.Max(i => i.OverdueDays) : 0;
            var debt = aging.TotalOutstandingDebt;
            var limit = customer.CreditLimit;
            var utilization = limit > 0 ? (debt / limit) * 100 : 100;

            int score = 100;
            var factors = new List<string>();

            // Phân tích nợ quá hạn
            if (aging.BucketOver90 > 0)
            {
                score -= 40;
                factors.Add($"Có khoản nợ nghiêm trọng {aging.BucketOver90:N0} đ quá hạn >90 ngày.");
            }
            else if (aging.Bucket61To90 > 0)
            {
                score -= 25;
                factors.Add($"Có khoản nợ {aging.Bucket61To90:N0} đ quá hạn từ 61-90 ngày.");
            }
            else if (aging.Bucket31To60 > 0)
            {
                score -= 15;
                factors.Add($"Có hóa đơn nợ {aging.Bucket31To60:N0} đ quá hạn từ 31-60 ngày.");
            }
            else if (maxOverdue > 0)
            {
                score -= 5;
                factors.Add($"Có hóa đơn trễ hạn nhẹ ({maxOverdue} ngày).");
            }

            // Phân tích tỷ lệ sử dụng hạn mức
            if (utilization >= 100)
            {
                score -= 30;
                factors.Add("Dư nợ đã chạm hoặc vượt trần hạn mức tín dụng được cấp.");
            }
            else if (utilization >= 80)
            {
                score -= 15;
                factors.Add($"Tỷ lệ sử dụng hạn mức đạt {utilization:N0}%, ở mức báo động cao.");
            }
            else if (utilization >= 50)
            {
                score -= 5;
                factors.Add($"Tỷ lệ sử dụng hạn mức đạt {utilization:N0}%.");
            }

            // Phân tích số lượng hóa đơn tồn đọng
            if (aging.Invoices.Count >= 5)
            {
                score -= 10;
                factors.Add($"Số lượng hóa đơn nợ chưa giải quyết nhiều ({aging.Invoices.Count} hóa đơn).");
            }

            score = Math.Clamp(score, 0, 100);

            string grade;
            string riskLevel;
            string recommendation;

            if (score >= 85)
            {
                grade = "A";
                riskLevel = "Rất Thấp (An Toàn)";
                recommendation = "Khách hàng uy tín cao, kỷ luật thanh toán tốt. Đủ điều kiện xem xét nâng hạn mức tín dụng và hưởng chính sách chiết khấu đối tác VIP.";
            }
            else if (score >= 65)
            {
                grade = "B";
                riskLevel = "Trung Bình (Chấp Nhận Được)";
                recommendation = "Khách hàng đáp ứng tiêu chuẩn tín dụng bình thường. Tiếp tục duy trì hạn mức và kỳ hạn thanh toán hiện tại.";
            }
            else if (score >= 40)
            {
                grade = "C";
                riskLevel = "Cao (Cần Kiểm Soát)";
                recommendation = "Cảnh báo rủi ro! Cần yêu cầu khách hàng thanh toán các khoản nợ quá hạn trước khi phê duyệt đơn hàng mua nợ mới.";
            }
            else
            {
                grade = "D";
                riskLevel = "Rất Cao (Báo Động Nợ Xấu)";
                recommendation = "NGỪNG BÁN CHỊU! Nguy cơ phát sinh nợ khó đòi cao. Tạm khóa quyền mua nợ, chỉ chấp nhận giao dịch thanh toán 100% tiền mặt trước khi xuất kho.";
            }

            var assessment = new CreditRiskAssessmentDto
            {
                CustomerId = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.FullName,
                CurrentDebt = debt,
                CreditLimit = limit,
                MaxOverdueDays = maxOverdue > 0 ? maxOverdue : 0,
                TotalUnpaidInvoices = aging.Invoices.Count,
                CreditScore = score,
                RatingGrade = grade,
                RiskLevel = riskLevel,
                Recommendation = recommendation,
                RiskFactors = factors
            };

            return Task.FromResult(assessment);
        }
    }
}
