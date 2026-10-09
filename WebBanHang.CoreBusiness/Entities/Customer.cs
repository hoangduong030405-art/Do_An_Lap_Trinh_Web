namespace WebBanHang.CoreBusiness.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
    }
}
