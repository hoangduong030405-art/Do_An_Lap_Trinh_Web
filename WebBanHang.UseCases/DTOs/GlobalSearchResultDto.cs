namespace WebBanHang.UseCases.DTOs
{
    public class GlobalSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Khách hàng" | "Hóa đơn" | "Sản phẩm"
        public string Url { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "primary";
    }
}
