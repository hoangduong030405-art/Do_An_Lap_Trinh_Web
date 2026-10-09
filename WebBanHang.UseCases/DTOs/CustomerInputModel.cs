using System.ComponentModel.DataAnnotations;

namespace WebBanHang.UseCases.DTOs
{
    public class CustomerInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã khách hàng không được để trống.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Mã khách hàng từ 3 đến 20 ký tự.")]
        public string CustomerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên khách hàng không được để trống.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên khách hàng từ 3 đến 150 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        public string Address { get; set; } = string.Empty;

        [Range(0, 1000000000, ErrorMessage = "Hạn mức tín dụng phải từ 0 đến 1.000.000.000 đ.")]
        public decimal CreditLimit { get; set; } = 10000000;

        [Range(1, 365, ErrorMessage = "Hạn nợ phải từ 1 đến 365 ngày.")]
        public int PaymentTermsDays { get; set; } = 30;
    }
}
