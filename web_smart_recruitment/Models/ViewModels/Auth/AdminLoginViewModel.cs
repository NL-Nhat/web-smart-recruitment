using System.ComponentModel.DataAnnotations;

namespace web_smart_recruitment.Models.ViewModels.Auth
{
    /// <summary>
    /// ViewModel dành riêng cho trang đăng nhập của Admin.
    /// Admin đăng nhập qua một trang riêng và không cần chọn vai trò vì mặc định là Admin.
    /// </summary>
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email Admin")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
