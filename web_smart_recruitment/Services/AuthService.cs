using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using web_smart_recruitment.Models;

namespace web_smart_recruitment.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo Access Token (JWT) có thời hạn ngắn (mặc định 15 phút).
        /// Token này chứa Role để phân quyền người dùng trong hệ thống.
        /// </summary>
        public string GenerateAccessToken(TaiKhoan account, string roleName)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            // Các Claims (thông tin đính kèm) trong Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.MaTaiKhoan.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("TokenType", "Access") // Phân biệt đây là Access Token
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15")),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo Refresh Token (JWT) có thời hạn dài (mặc định 7 ngày).
        /// Token này được dùng để cấp lại Access Token mới khi Access Token cũ hết hạn.
        /// Chú ý: Không chứa Role để giảm thiểu rủi ro nếu bị lộ.
        /// </summary>
        public string GenerateRefreshToken(TaiKhoan account)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.MaTaiKhoan.ToString()),
                new Claim("TokenType", "Refresh")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7")),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Giải mã một Token đã hết hạn để lấy lại các thông tin Claims.
        /// Dùng trong Middleware để thực hiện quy trình Refresh Token tự động.
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                ValidateLifetime = false // Cho phép đọc token ngay cả khi đã hết hạn
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Băm mật khẩu người dùng bằng thuật toán BCrypt.
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Kiểm tra mật khẩu người dùng nhập vào so với mã hash trong Database.
        /// Sử dụng thư viện BCrypt đảm bảo tính bảo mật cao nhất hiện nay.
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Hàm Verify của BCrypt sẽ tự động xử lý salt và so khớp
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Nếu dữ liệu trong DB không phải chuẩn BCrypt hash, hàm sẽ ném lỗi
                return false;
            }
        }
    }
}
