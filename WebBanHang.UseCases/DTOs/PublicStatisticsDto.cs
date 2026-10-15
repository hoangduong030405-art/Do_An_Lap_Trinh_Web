namespace WebBanHang.UseCases.DTOs
{
    public class PublicStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalInvoicesProcessed { get; set; }
        public decimal DebtCollectionRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCollectedAmount { get; set; }
    }
}
