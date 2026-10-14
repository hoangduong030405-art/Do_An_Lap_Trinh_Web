namespace WebBanHang.CoreBusiness.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Tính toán đối chiếu giá nhập vs giá bán sỉ
        public decimal UnitProfit => UnitPrice - CostPrice;
        public decimal ProfitMargin => UnitPrice > 0 ? Math.Round(((UnitPrice - CostPrice) / UnitPrice) * 100, 1) : 0;
    }
}
