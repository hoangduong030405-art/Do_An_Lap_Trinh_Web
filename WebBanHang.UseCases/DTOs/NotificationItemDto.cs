using System;

namespace WebBanHang.UseCases.DTOs
{
    public class NotificationItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // "danger" | "warning" | "info" | "success"
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string Icon { get; set; } = "bi-bell";
    }
}
