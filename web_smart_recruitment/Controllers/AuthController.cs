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

        /// <summary>
        /// Hiển thị trang đăng ký tài khoản.
        /// Load danh sách vai trò từ DB để người dùng lựa chọn (Ứng viên hoặc Nhà tuyển dụng).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var roles = await _context.VaiTros
                .Where(r => r.TenVaiTro != "Admin")
                .ToListAsync();
            
            ViewBag.Roles = roles;
            return View();
        }

        /// <summary>
        /// Xử lý logic đăng ký tài khoản mới.
        /// Quy trình: Băm mật khẩu -> Lưu bảng TaiKhoan -> Lưu bảng con (UngVien hoặc NhaTuyenDung).
        /// Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // 1. Kiểm tra Email đã tồn tại chưa
            var existingUser = await _context.TaiKhoans.AnyAsync(a => a.Email == model.Email);
            if (existingUser)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // 2. Kiểm tra Số điện thoại đã tồn tại trong hệ thống chưa
            // Kiểm tra ở cả bảng UngVien và NhaTuyenDung để đảm bảo tính duy nhất
            var phoneExistsInUngVien = await _context.UngViens.AnyAsync(uv => uv.SoDienThoai == model.SoDienThoai);
            var phoneExistsInNhaTuyenDung = await _context.NhaTuyenDungs.AnyAsync(ntd => ntd.SoDienThoai == model.SoDienThoai);
            
            if (phoneExistsInUngVien || phoneExistsInNhaTuyenDung)
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng bởi một tài khoản khác.");
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // 3. Lấy mã vai trò từ tên vai trò
            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == model.VaiTro);
            if (role == null)
            {
                ModelState.AddModelError("", "Vai trò không hợp lệ.");
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }

            // Sử dụng Transaction (Giao dịch) để đảm bảo tính toàn vẹn dữ liệu:
            // Nếu việc tạo bản ghi ở bảng con (UngVien/NhaTuyenDung) thất bại, 
            // tài khoản ở bảng TaiKhoan cũng sẽ không được tạo.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Khởi tạo đối tượng TaiKhoan mới
                var newAccount = new TaiKhoan
                {
                    Email = model.Email,
                    // Sử dụng AuthService để băm mật khẩu bảo mật trước khi lưu vào DB
                    MatKhauHash = _authService.HashPassword(model.Password), 
                    MaVaiTro = role.MaVaiTro,
                    TrangThaiHoatDong = true, // Mặc định tài khoản mới sẽ được hoạt động ngay
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };

                _context.TaiKhoans.Add(newAccount);
                // Lưu để lấy MaTaiKhoan (Identity) dùng làm khóa ngoại cho bảng con
                await _context.SaveChangesAsync();

                // 4. Dựa vào vai trò người dùng chọn để tạo thông tin chi tiết tương ứng
                if (model.VaiTro == "UngVien")
                {
                    // Tạo bản ghi cho bảng Ứng viên
                    var ungVien = new UngVien
                    {
                        MaUngVien = newAccount.MaTaiKhoan, // PK-FK 1:1 với bảng TaiKhoan
                        HoTen = model.HoTen,
                        SoDienThoai = model.SoDienThoai
                    };
                    _context.UngViens.Add(ungVien);
                }
                else if (model.VaiTro == "NhaTuyenDung")
                {
                    // Tạo bản ghi cho bảng Nhà tuyển dụng
                    var nhaTuyenDung = new NhaTuyenDung
                    {
                        MaNhaTuyenDung = newAccount.MaTaiKhoan, // PK-FK 1:1 với bảng TaiKhoan
                        HoTen = model.HoTen,
                        TenCongTy = model.TenCongTy,
                        SoDienThoai = model.SoDienThoai
                    };
                    _context.NhaTuyenDungs.Add(nhaTuyenDung);
                }

                // Lưu các thay đổi ở bảng con
                await _context.SaveChangesAsync();

                // Xác nhận hoàn tất giao dịch thành công
                await transaction.CommitAsync();

                // 5. Thông báo cho người dùng và chuyển hướng về trang đăng nhập
                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Bây giờ bạn có thể đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào, hủy bỏ toàn bộ các thay đổi trong Database
                await transaction.RollbackAsync();
                
                // Ghi log lỗi nếu cần thiết (ở đây ta hiển thị thông báo chung)
                ModelState.AddModelError("", "Hệ thống đang bận, vui lòng thử lại sau.");
                
                // Load lại danh sách vai trò để hiển thị lại View
                ViewBag.Roles = await _context.VaiTros.Where(r => r.TenVaiTro != "Admin").ToListAsync();
                return View(model);
            }
        }
    }
}
