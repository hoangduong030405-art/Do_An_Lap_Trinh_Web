using System;

namespace WebBanHang.UseCases.DTOs
{
    public class SmartReminderDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Thông tin"; // "Khẩn" | "Cần chú ý" | "Thông tin"
        public string PriorityBadge { get; set; } = "info"; // "danger" | "warning" | "info"
        public string ActionType { get; set; } = "general"; // "email" | "credit_risk" | "invoice" | "product"
        public int ReferenceId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsDismissed { get; set; } = false;
    }

    public class AvaAiAnswerDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string[] SuggestedActions { get; set; } = Array.Empty<string>();
    }

    public class DebtReminderEmailDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
