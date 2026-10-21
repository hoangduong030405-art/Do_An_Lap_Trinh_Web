using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.UseCases.DTOs;

namespace WebBanHang.UseCases.AI
{
    public class MockAiDebtReminderService : IAiDebtReminderService
    {
        public Task<string> GenerateReminderEmailAsync(CustomerAgingReportDto agingReport)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"Kính gửi Quý khách {agingReport.CustomerName},");
            sb.AppendLine();
            sb.AppendLine($"Công ty xin thông báo quý khách hiện có các hóa đơn chưa thanh toán với tổng số tiền {agingReport.TotalOutstandingDebt:N0} đ.");
            
            if (agingReport.BucketOver90 > 0 || agingReport.Bucket61To90 > 0 || agingReport.Bucket31To60 > 0 || agingReport.Bucket1To30 > 0)
            {
                sb.AppendLine("Trong đó, số dư nợ theo thời gian quá hạn như sau:");
                if (agingReport.BucketOver90 > 0) sb.AppendLine($"- Quá hạn > 90 ngày: {agingReport.BucketOver90:N0} đ");
                if (agingReport.Bucket61To90 > 0) sb.AppendLine($"- Quá hạn 61-90 ngày: {agingReport.Bucket61To90:N0} đ");
                if (agingReport.Bucket31To60 > 0) sb.AppendLine($"- Quá hạn 31-60 ngày: {agingReport.Bucket31To60:N0} đ");
                if (agingReport.Bucket1To30 > 0) sb.AppendLine($"- Quá hạn 1-30 ngày: {agingReport.Bucket1To30:N0} đ");
                if (agingReport.BucketNotDue > 0) sb.AppendLine($"- Chưa đến hạn (trong hạn): {agingReport.BucketNotDue:N0} đ");
            }

            sb.AppendLine();
            sb.AppendLine("Chi tiết các hóa đơn cần lưu ý:");
            
            var overdueInvoices = agingReport.Invoices.Where(i => i.OverdueDays > 0).OrderByDescending(i => i.OverdueDays).ToList();
            
            if (overdueInvoices.Any())
            {
                foreach (var inv in overdueInvoices)
                {
                    sb.AppendLine($"- HĐ số: {inv.InvoiceNumber}, Ngày đến hạn: {inv.DueDate:dd/MM/yyyy}, Số tiền nợ: {inv.Balance:N0} đ (Quá hạn {inv.OverdueDays} ngày)");
                }
            }
            else
            {
                sb.AppendLine("- Hiện tất cả hóa đơn vẫn đang trong kỳ hạn thanh toán an toàn.");
            }

            sb.AppendLine();
            sb.AppendLine("Rất mong Quý khách sắp xếp thanh toán sớm qua Số Tài Khoản: 35782747 - Ngân hàng TMCP Á Châu (ACB).");
            sb.AppendLine();
            sb.AppendLine("Trân trọng cảm ơn!");
            sb.AppendLine("Phòng Kế Toán - Công Ty XYZ");

            return Task.FromResult(sb.ToString());
        }
    }
}
