using System.ComponentModel.DataAnnotations;

namespace WebBanHang.UseCases.DTOs
{
    public class ProductInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Mã sản phẩm từ 2 đến 20 ký tự.")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 200 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị tính không được để trống.")]
        [StringLength(20, ErrorMessage = "Đơn vị tính không quá 20 ký tự.")]
        public string Unit { get; set; } = "Cái";

        [Range(0, 1000000000, ErrorMessage = "Giá bán phải từ 0 đến 1.000.000.000 đ.")]
        public decimal UnitPrice { get; set; } = 0;

        [Range(0, 100000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 100.000.")]
        public int StockQuantity { get; set; } = 0;

        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
