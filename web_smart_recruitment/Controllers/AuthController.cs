using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.ViewModels.Auth;
using web_smart_recruitment.Services;

namespace web_smart_recruitment.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Hiển thị trang đăng nhập cho Ứng viên và Nhà tuyển dụng.
        /// Tự động load danh sách vai trò từ database để hiển thị lên giao diện.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Lấy danh sách vai trò (trừ Admin) để hiển thị ở trang đăng nhập người dùng
            var roles = await _context.VaiTros
                .Where(r => r.TenVaiTro != "Admin")
                .ToListAsync();
            
            ViewBag.Roles = roles;
            return View();
        }

        /// <summary>
        /// Xử lý logic đăng nhập cho người dùng thường.
        /// Kiểm tra Email, Mật khẩu (hash) và Vai trò tương ứng.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // Tìm tài khoản khớp cả Email và Vai trò đã chọn
            var account = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .FirstOrDefaultAsync(a => a.Email == model.Email && a.MaVaiTroNavigation.TenVaiTro == model.VaiTro);

            // Kiểm tra mật khẩu bằng BCrypt
            if (account == null || !_authService.VerifyPassword(model.Password, account.MatKhauHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            if (account.TrangThaiHoatDong == false)
            {
                ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị khóa bởi quản trị viên.");
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // Đăng nhập thành công -> Cấp Token vào Cookie
            SetAuthCookies(account, account.MaVaiTroNavigation.TenVaiTro);

            // Điều hướng người dùng về trang chủ của từng vai trò
            if (account.MaVaiTroNavigation.TenVaiTro == "UngVien")
                return RedirectToAction("Jobs", "Candidate");
            
            return RedirectToAction("Dashboard", "Hr");
        }

        /// <summary>
        /// Trang đăng nhập dành riêng cho quản trị viên hệ thống (Admin).
        /// </summary>
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập Admin. Bảo mật cao hơn bằng cách chỉ cho phép các tài khoản có Role 'Admin'.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .FirstOrDefaultAsync(a => a.Email == model.Email && a.MaVaiTroNavigation.TenVaiTro == "Admin");

            if (account == null || !_authService.VerifyPassword(model.Password, account.MatKhauHash))
            {
                ModelState.AddModelError("", "Thông tin đăng nhập Admin không hợp lệ.");
                return View(model);
            }

            SetAuthCookies(account, "Admin");
            return RedirectToAction("Dashboard", "Admin");
        }

        /// <summary>
        /// Hàm nội bộ để lưu trữ Token vào HttpOnly Cookie.
        /// Cookie này không thể bị truy cập bởi JavaScript, giúp chống lại tấn công XSS.
        /// </summary>
        private void SetAuthCookies(TaiKhoan account, string roleName)
        {
            var accessToken = _authService.GenerateAccessToken(account, roleName);
            var refreshToken = _authService.GenerateRefreshToken(account);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Yêu cầu HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
