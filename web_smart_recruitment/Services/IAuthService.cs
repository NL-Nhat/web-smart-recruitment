using System.Security.Claims;
using web_smart_recruitment.Models;

namespace web_smart_recruitment.Services
{
    /// <summary>
    /// Giao diện định nghĩa các phương thức xử lý xác thực:
    /// - Tạo Access Token và Refresh Token.
    /// - Giải mã Token.
    /// - Kiểm tra mật khẩu mã hóa.
    /// </summary>
    public interface IAuthService
    {
        string GenerateAccessToken(TaiKhoan account, string roleName);
        string GenerateRefreshToken(TaiKhoan account);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
