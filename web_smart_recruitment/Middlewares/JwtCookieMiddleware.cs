using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_smart_recruitment.Models;
using web_smart_recruitment.Services;

namespace web_smart_recruitment.Middlewares
{
    /// <summary>
    /// Middleware quan trọng xử lý cơ chế "Stateless Refresh Token":
    /// - Kiểm tra AccessToken trong Cookie.
    /// - Nếu hết hạn, kiểm tra RefreshToken để tự động cấp mới mà không bắt người dùng đăng nhập lại.
    /// - Đảm bảo hiệu năng cao vì không truy cập DB nếu AccessToken còn hạn.
    /// </summary>
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService, AppDbContext dbContext)
        {
            var accessToken = context.Request.Cookies["AccessToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                // Giải mã Token để kiểm tra thông tin
                var principal = authService.GetPrincipalFromExpiredToken(accessToken);
                
                if (principal != null)
                {
                    var expClaim = principal.FindFirst("exp");
                    if (expClaim != null)
                    {
                        var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime;
                        
                        // Nếu Access Token còn hạn sử dụng
                        if (expTime > DateTime.UtcNow)
                        {
                            context.User = principal;
                        }
                        else
                        {
                            // Nếu hết hạn, thực hiện quy trình Refresh
                            await HandleRefreshToken(context, authService, dbContext);
                        }
                    }
                }
            }
            else
            {
                // Trường hợp chưa có Access Token nhưng có thể có Refresh Token
                await HandleRefreshToken(context, authService, dbContext);
            }

            await _next(context);
        }

        /// <summary>
        /// Xử lý logic dùng Refresh Token để cấp lại Access Token mới.
        /// Chú ý: Chỉ truy xuất DB ở bước này để kiểm tra trạng thái tài khoản.
        /// </summary>
        private async Task HandleRefreshToken(HttpContext context, IAuthService authService, AppDbContext dbContext)
        {
            var refreshToken = context.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return;

            var principal = authService.GetPrincipalFromExpiredToken(refreshToken);
            if (principal == null) return;

            // Kiểm tra xem token có phải là loại Refresh không
            var typeClaim = principal.FindFirst("TokenType");
            if (typeClaim == null || typeClaim.Value != "Refresh") return;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return;

            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                // Truy vấn DB để chắc chắn tài khoản vẫn tồn tại và không bị khóa
                var account = await dbContext.TaiKhoans
                    .Include(a => a.MaVaiTroNavigation)
                    .FirstOrDefaultAsync(a => a.MaTaiKhoan == userId && a.TrangThaiHoatDong == true);

                if (account != null)
                {
                    // Lấy Họ tên tương ứng
                    string fullName = "Người dùng";
                    if (account.MaVaiTroNavigation.TenVaiTro == "UngVien")
                    {
                        var uv = await dbContext.UngViens.FirstOrDefaultAsync(u => u.MaUngVien == account.MaTaiKhoan);
                        fullName = uv?.HoTen ?? account.Email;
                    }
                    else if (account.MaVaiTroNavigation.TenVaiTro == "NhaTuyenDung")
                    {
                        var ntd = await dbContext.NhaTuyenDungs.FirstOrDefaultAsync(n => n.MaNhaTuyenDung == account.MaTaiKhoan);
                        fullName = ntd?.HoTen ?? account.Email;
                    }
                    else if (account.MaVaiTroNavigation.TenVaiTro == "Admin")
                    {
                        fullName = "Quản trị viên";
                    }

                    // Tạo bộ đôi Token mới
                    var newAccessToken = authService.GenerateAccessToken(account, account.MaVaiTroNavigation.TenVaiTro, fullName);
                    var newRefreshToken = authService.GenerateRefreshToken(account);

                    // Thiết lập Cookie bảo mật (HttpOnly)
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Chỉ gửi qua HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7) // Theo hạn của Refresh Token
                    };

                    context.Response.Cookies.Append("AccessToken", newAccessToken, cookieOptions);
                    context.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

                    // Gán User vào Context để các Controller/View sử dụng ngay lập tức
                    context.User = authService.GetPrincipalFromExpiredToken(newAccessToken);
                }
            }
        }
    }
}
