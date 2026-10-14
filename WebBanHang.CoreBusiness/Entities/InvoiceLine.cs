namespace WebBanHang.CoreBusiness.Entities
{
    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public decimal LineCost => Quantity * CostPrice;
        public decimal LineProfit => LineTotal - LineCost;
    }
}
