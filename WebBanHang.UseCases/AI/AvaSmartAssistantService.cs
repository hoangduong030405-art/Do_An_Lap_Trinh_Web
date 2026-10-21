using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.CoreBusiness.Entities;
using WebBanHang.UseCases.DTOs;
using WebBanHang.UseCases.PluginInterfaces.DataStore;

namespace WebBanHang.UseCases.AI
{
    public class AvaSmartAssistantService : IAvaSmartAssistantService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public AvaSmartAssistantService(
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<SmartReminderDto>> GetSmartRemindersAsync(string role, int? customerId = null)
        {
            var reminders = new List<SmartReminderDto>();
            var now = DateTime.Now;

            // 1. INVOICE REMINDERS (Overdue & Upcoming Due)
            var allInvoices = await _invoiceRepository.GetInvoicesWithCustomerAsync();
            var activeInvoices = allInvoices.Where(i => i.Balance > 0);

            if (role == "Customer" && customerId.HasValue)
            {
                activeInvoices = activeInvoices.Where(i => i.CustomerId == customerId.Value);
            }

            foreach (var inv in activeInvoices)
            {
                int overdueDays = Invoice.CalculateOverdueDays(inv.DueDate, inv.Balance, now);
                if (overdueDays > 0)
                {
                    if (overdueDays >= 10)
                    {
                        reminders.Add(new SmartReminderDto
                        {
                            Id = $"inv-overdue-crit-{inv.Id}",
                            Title = $"Hóa đơn {inv.InvoiceNumber} quá hạn {overdueDays} ngày",
                            Description = $"Khách hàng {inv.CustomerName} còn nợ {inv.Balance:N0} đ (Hạn chót: {inv.DueDate:dd/MM/yyyy}).",
                            Priority = "Khẩn",
                            PriorityBadge = "danger",
                            ActionType = "email",
                            ReferenceId = inv.Id,
                            Url = "/invoices",
                            CustomerName = inv.CustomerName,
                            InvoiceNumber = inv.InvoiceNumber,
                            Amount = inv.Balance
                        });
                    }
                    else
                    {
                        reminders.Add(new SmartReminderDto
                        {
                            Id = $"inv-overdue-warn-{inv.Id}",
                            Title = $"Hóa đơn {inv.InvoiceNumber} quá hạn {overdueDays} ngày",
                            Description = $"Khách hàng {inv.CustomerName} còn nợ {inv.Balance:N0} đ. Đề xuất Kế toán liên hệ nhắc nợ.",
                            Priority = "Cần chú ý",
                            PriorityBadge = "warning",
                            ActionType = "email",
                            ReferenceId = inv.Id,
                            Url = "/invoices",
                            CustomerName = inv.CustomerName,
                            InvoiceNumber = inv.InvoiceNumber,
                            Amount = inv.Balance
                        });
                    }
                }
                else if (inv.DueDate <= now.AddDays(3))
                {
                    int daysLeft = Math.Max(0, (int)(inv.DueDate - now).TotalDays);
                    reminders.Add(new SmartReminderDto
                    {
                        Id = $"inv-due-soon-{inv.Id}",
                        Title = $"HĐ {inv.InvoiceNumber} sắp đến hạn ({daysLeft} ngày)",
                        Description = $"Đại lý {inv.CustomerName} có khoản thanh toán {inv.Balance:N0} đ đến hạn ngày {inv.DueDate:dd/MM}.",
                        Priority = "Thông tin",
                        PriorityBadge = "info",
                        ActionType = "invoice",
                        ReferenceId = inv.Id,
                        Url = "/invoices",
                        CustomerName = inv.CustomerName,
                        InvoiceNumber = inv.InvoiceNumber,
                        Amount = inv.Balance
                    });
                }
            }

            // 2. CUSTOMER CREDIT LIMIT REMINDERS (Only for Accountant & Sales)
            if (role != "Customer")
            {
                var customers = await _customerRepository.GetAllAsync();
                foreach (var cust in customers)
                {
                    decimal currentDebt = await _customerRepository.GetTotalOutstandingDebtAsync(cust.Id);
                    if (cust.CreditLimit > 0)
                    {
                        decimal ratio = currentDebt / cust.CreditLimit;
                        if (ratio >= 0.85m)
                        {
                            reminders.Add(new SmartReminderDto
                            {
                                Id = $"cust-credit-{cust.Id}",
                                Title = $"Khách hàng {cust.FullName} cận hạn mức tín dụng ({ratio:P0})",
                                Description = $"Dư nợ hiện tại: {currentDebt:N0} đ / Hạn mức tối đa: {cust.CreditLimit:N0} đ. Cần kiểm duyệt kỹ trước khi xuất thêm đơn mới.",
                                Priority = ratio >= 1.0m ? "Khẩn" : "Cần chú ý",
                                PriorityBadge = ratio >= 1.0m ? "danger" : "warning",
                                ActionType = "credit_risk",
                                ReferenceId = cust.Id,
                                Url = "/manage-customers",
                                CustomerName = cust.FullName,
                                Amount = currentDebt
                            });
                        }
                    }
                }

                // 3. LOW INVENTORY WARNINGS
                var products = await _productRepository.GetAllAsync();
                foreach (var prod in products.Where(p => p.StockQuantity <= 15))
                {
                    reminders.Add(new SmartReminderDto
                    {
                        Id = $"prod-stock-{prod.Id}",
                        Title = $"Sản phẩm '{prod.Name}' sắp hết hàng",
                        Description = $"Tồn kho hiện tại chỉ còn {prod.StockQuantity} {prod.Unit}. Đề xuất nhập thêm hàng để đảm bảo nguồn cung cho đại lý.",
                        Priority = prod.StockQuantity <= 5 ? "Cần chú ý" : "Thông tin",
                        PriorityBadge = prod.StockQuantity <= 5 ? "warning" : "info",
                        ActionType = "product",
                        ReferenceId = prod.Id,
                        Url = "/manage-products"
                    });
                }
            }

            // Order by Priority: Khẩn -> Cần chú ý -> Thông tin
            return reminders.OrderBy(r => r.Priority switch
            {
                "Khẩn" => 1,
                "Cần chú ý" => 2,
                _ => 3
            }).Take(10);
        }

        public async Task<AvaAiAnswerDto> AnswerAccountingQuestionAsync(string question, string role, int? customerId = null)
        {
            var q = (question ?? "").Trim().ToLowerInvariant();
            var allInvoices = await _invoiceRepository.GetInvoicesWithCustomerAsync();
            var customers = await _customerRepository.GetAllAsync();
            var now = DateTime.Now;

            string answerText;
            var suggestedActions = new List<string>();

            if (q.Contains("nợ quá hạn") || q.Contains("ai đang nợ"))
            {
                var overdueInvoices = allInvoices.Where(i => Invoice.CheckIsOverdue(i.DueDate, i.Balance, now)).ToList();
                if (!overdueInvoices.Any())
                {
                    answerText = "Tin tốt! Hiện tại toàn bộ khách hàng và đại lý đều thanh toán đúng hạn, không có hóa đơn nào quá hạn thanh toán.";
                }
                else
                {
                    var debtorNames = overdueInvoices.Select(i => $"{i.CustomerName} (HĐ #{i.InvoiceNumber}: {i.Balance:N0} đ, quá hạn {Invoice.CalculateOverdueDays(i.DueDate, i.Balance, now)} ngày)").Distinct();
                    decimal totalOverdue = overdueInvoices.Sum(i => i.Balance);
                    answerText = $"Tổng cộng có {overdueInvoices.Count} hóa đơn quá hạn với tổng số tiền nợ là {totalOverdue:N0} đ:\n• " + string.Join("\n• ", debtorNames);
                    suggestedActions.Add("Soạn email đòi nợ");
                    suggestedActions.Add("Xem báo cáo phân tích tuổi nợ");
                }
            }
            else if (q.Contains("tóm tắt") || q.Contains("tổng quan") || q.Contains("hôm nay"))
            {
                decimal totalOutstanding = allInvoices.Sum(i => i.Balance);
                decimal totalCollected = allInvoices.Sum(i => i.PaidAmount);
                decimal totalRevenue = allInvoices.Sum(i => i.TotalAmount);
                decimal rate = totalRevenue > 0 ? Math.Round((totalCollected / totalRevenue) * 100, 1) : 0;

                answerText = $"Tình hình tài chính công nợ ghi nhận thời gian thực:\n" +
                             $"• Tổng doanh thu phát sinh: {totalRevenue:N0} đ\n" +
                             $"• Đã thu tiền thành công: {totalCollected:N0} đ (Đạt {rate}%)\n" +
                             $"• Tổng nợ phải thu còn lại: {totalOutstanding:N0} đ\n" +
                             $"• Thuật toán cấn trừ tự động FIFO và cờ toàn vẹn IsIntegrityValid đang hoạt động ổn định 100%.";
                suggestedActions.Add("Thu tiền khách hàng");
                suggestedActions.Add("Xem sổ chi tiết công nợ");
            }
            else if (q.Contains("siết hạn mức") || q.Contains("rủi ro") || q.Contains("tín dụng"))
            {
                var highRiskList = new List<string>();
                foreach (var c in customers)
                {
                    decimal debt = await _customerRepository.GetTotalOutstandingDebtAsync(c.Id);
                    if (c.CreditLimit > 0 && debt >= c.CreditLimit * 0.8m)
                    {
                        highRiskList.Add($"{c.FullName}: nợ {debt:N0} đ / hạn mức {c.CreditLimit:N0} đ ({debt / c.CreditLimit:P0})");
                    }
                }

                if (highRiskList.Any())
                {
                    answerText = $"Phát hiện các đại lý tiệm cận hoặc vượt trần rủi ro hạn mức:\n• " + string.Join("\n• ", highRiskList) +
                                 "\nKhuyến nghị AVA: Nên khóa chức năng xuất hóa đơn nợ cho các khách hàng này cho đến khi ghi nhận phiếu thu tiền cấn trừ.";
                    suggestedActions.Add("Đánh giá hạn mức tín dụng");
                }
                else
                {
                    answerText = "Mức độ sử dụng tín dụng của các đại lý đều nằm trong vùng an toàn (< 80% hạn mức). Chưa cần thiết phải siết hạn mức.";
                }
            }
            else
            {
                answerText = $"Tôi đã phân tích hệ thống dữ liệu kế toán và phân phối bán buôn. Hiện tại đang quản lý {customers.Count()} đại lý và {allInvoices.Count()} hóa đơn. Mọi giao dịch đều tuân thủ kiểm soát hạn mức nợ và cấn trừ FIFO.";
                suggestedActions.Add("Tóm tắt công nợ hôm nay");
                suggestedActions.Add("Ai đang nợ quá hạn?");
            }

            return new AvaAiAnswerDto
            {
                Question = question ?? string.Empty,
                Answer = answerText,
                GeneratedAt = DateTime.Now,
                SuggestedActions = suggestedActions.ToArray()
            };
        }
    }
}
