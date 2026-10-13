namespace WebBanHang.UseCases.DTOs
{
    public class CustomerPortalOrderResultDto
    {
        public bool Success { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal RemainingAvailableCredit { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
