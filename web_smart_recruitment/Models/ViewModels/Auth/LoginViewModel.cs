using System.ComponentModel.DataAnnotations;

namespace web_smart_recruitment.Models.ViewModels.Auth
{
    /// <summary>
    /// ViewModel dùng cho trang đăng nhập của Ứng viên và Nhà tuyển dụng.
    /// Bao gồm các trường: Email, Mật khẩu và Vai trò để phân loại người dùng.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty; // Lưu tên vai trò (UngVien, NhaTuyenDung)
    }
}
