using System.ComponentModel.DataAnnotations;

namespace web_smart_recruitment.Models.ViewModels.Auth
{
    /// <summary>
    /// ViewModel hỗ trợ đăng ký cho cả Ứng viên và Nhà tuyển dụng.
    /// Có các trường chung và các trường đặc thù tùy theo vai trò.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Các trường dành riêng cho Nhà tuyển dụng
        [StringLength(150, ErrorMessage = "Tên công ty không được vượt quá 150 ký tự")]
        public string? TenCongTy { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có đúng 10 chữ số")]
        public string? SoDienThoai { get; set; }
    }
}
