using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.Dtos;
using web_smart_recruitment.Models.RequestModels.Admin;
using web_smart_recruitment.Models.ViewModels;
using web_smart_recruitment.Services;
using web_smart_recruitment.Enums;

namespace web_smart_recruitment.Controllers
{
    /// <summary>
    /// Controller quản lý toàn bộ chức năng của Admin:
    /// Quản lý người dùng, kỹ năng, vai trò, xem báo cáo và xuất Excel.
    /// Tất cả action yêu cầu đăng nhập với quyền Admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AdminController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public IActionResult Login() => View();

        // =====================================================================
        // DASHBOARD
        // =====================================================================

        /// <summary>
        /// Trang tổng quan: hiển thị số liệu thống kê cơ bản của hệ thống.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            // Đếm tổng số từng loại bản ghi — dùng LINQ Count tối ưu, không load dữ liệu thừa
            ViewBag.CountUsers = await _context.TaiKhoans.CountAsync();
            ViewBag.CountHr    = await _context.NhaTuyenDungs.CountAsync();
            ViewBag.CountJobs  = await _context.TinTuyenDungs.CountAsync();
            ViewBag.CountApps  = await _context.DonUngTuyens.CountAsync();

            return View();
        }

        // =====================================================================
        // QUẢN LÝ NGƯỜI DÙNG
        // =====================================================================

        /// <summary>
        /// Hiển thị danh sách toàn bộ tài khoản trong hệ thống.
        /// Dùng LINQ Join để lấy Họ tên hiển thị theo từng vai trò một cách tối ưu.
        /// </summary>
        public async Task<IActionResult> Users()
        {
            // Join với UngVien/NhaTuyenDung để lấy tên hiển thị mà không cần N+1 query
            var ungVienNames = await _context.UngViens
                .ToDictionaryAsync(u => u.MaUngVien, u => u.HoTen ?? "Chưa cập nhật");

            var employerNames = await _context.NhaTuyenDungs
                .ToDictionaryAsync(n => n.MaNhaTuyenDung, n => n.TenCongTy ?? "Chưa cập nhật");

            var users = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .Select(a => new AdminUserViewModel
                {
                    MaTaiKhoan       = a.MaTaiKhoan,
                    Email            = a.Email,
                    TenVaiTro        = a.MaVaiTroNavigation.TenVaiTro,
                    TrangThaiHoatDong = a.TrangThaiHoatDong ?? true,
                    // TenHienThi được resolve ở bước sau (sau khi ToListAsync)
                    TenHienThi = a.MaVaiTroNavigation.TenVaiTro
                })
                .ToListAsync();

            // Resolve tên hiển thị dựa vào role — thực hiện ở phía .NET để tránh lỗi EF translate
            foreach (var user in users)
            {
                user.TenHienThi = user.TenVaiTro switch
                {
                    "UngVien"      => ungVienNames.TryGetValue(user.MaTaiKhoan, out var n1) ? n1 : "Chưa cập nhật",
                    "NhaTuyenDung" => employerNames.TryGetValue(user.MaTaiKhoan, out var n2) ? n2 : "Chưa cập nhật",
                    _              => "Quản trị viên"
                };
            }

            return View(users);
        }

        /// <summary>
        /// Thêm tài khoản người dùng mới.
        /// Nếu là UngVien hoặc NhaTuyenDung, tự động tạo bản ghi profile tương ứng.
        /// Dùng transaction để đảm bảo tính toàn vẹn dữ liệu.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return Json(new { success = false, message = "Email và mật khẩu không được trống." });

            if (await _context.TaiKhoans.AnyAsync(a => a.Email == model.Email))
                return Json(new { success = false, message = "Email này đã tồn tại trong hệ thống." });

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == model.VaiTro);
            if (role == null)
                return Json(new { success = false, message = "Vai trò không hợp lệ." });

            // Dùng transaction để đảm bảo TaiKhoan và UngVien/NhaTuyenDung được tạo cùng lúc
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newAccount = new TaiKhoan
                {
                    Email             = model.Email,
                    MatKhauHash       = _authService.HashPassword(model.Password),
                    MaVaiTro          = role.MaVaiTro,
                    TrangThaiHoatDong = true,
                    NgayTao           = DateTime.Now,
                    NgayCapNhat       = DateTime.Now
                };

                _context.TaiKhoans.Add(newAccount);
                await _context.SaveChangesAsync();

                // Tạo profile tương ứng với vai trò
                if (model.VaiTro == "UngVien")
                {
                    _context.UngViens.Add(new UngVien
                    {
                        MaUngVien  = newAccount.MaTaiKhoan,
                        HoTen      = model.HoTenCandidate ?? "Ứng viên mới",
                        SoDienThoai = model.SoDienThoaiCandidate
                    });
                }
                else if (model.VaiTro == "NhaTuyenDung")
                {
                    _context.NhaTuyenDungs.Add(new NhaTuyenDung
                    {
                        MaNhaTuyenDung = newAccount.MaTaiKhoan,
                        TenCongTy      = model.TenCongTy ?? "Công ty mới",
                        SoDienThoai    = model.SoDienThoaiEmployer,
                        DiaChi         = model.DiaChi
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Thêm người dùng mới thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật vai trò của tài khoản.
        /// Nếu đổi sang vai trò UngVien/NhaTuyenDung mà chưa có profile, tự động tạo profile mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateRoleRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var account = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .FirstOrDefaultAsync(a => a.MaTaiKhoan == model.MaTaiKhoan);

            if (account == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            var newRole = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == model.TenVaiTro);
            if (newRole == null)
                return Json(new { success = false, message = "Vai trò mới không hợp lệ." });

            // Không cần cập nhật nếu vai trò không đổi
            if (account.MaVaiTro == newRole.MaVaiTro)
                return Json(new { success = true, message = "Vai trò không thay đổi." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                account.MaVaiTro    = newRole.MaVaiTro;
                account.NgayCapNhat = DateTime.Now;

                // Tạo profile nếu chưa có khi đổi vai trò
                if (model.TenVaiTro == "UngVien" && !await _context.UngViens.AnyAsync(u => u.MaUngVien == account.MaTaiKhoan))
                {
                    _context.UngViens.Add(new UngVien { MaUngVien = account.MaTaiKhoan, HoTen = "Ứng viên mới" });
                }
                else if (model.TenVaiTro == "NhaTuyenDung" && !await _context.NhaTuyenDungs.AnyAsync(n => n.MaNhaTuyenDung == account.MaTaiKhoan))
                {
                    _context.NhaTuyenDungs.Add(new NhaTuyenDung { MaNhaTuyenDung = account.MaTaiKhoan, TenCongTy = "Công ty mới" });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Cập nhật vai trò thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Khóa hoặc mở khóa tài khoản người dùng (toggle trạng thái hoạt động).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleStatusRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(a => a.MaTaiKhoan == model.MaTaiKhoan);
            if (account == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            // Toggle trạng thái: đang mở thì khóa, đang khóa thì mở
            account.TrangThaiHoatDong = !(account.TrangThaiHoatDong ?? true);
            account.NgayCapNhat       = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                string statusText = account.TrangThaiHoatDong.Value ? "mở khóa" : "khóa";
                return Json(new { success = true, message = $"Đã {statusText} tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // QUẢN LÝ KỸ NĂNG
        // =====================================================================

        public async Task<IActionResult> Skills()
        {
            var skills = await _context.DanhMucKyNangs.ToListAsync();
            return View(skills);
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenKyNang))
                return Json(new { success = false, message = "Tên kỹ năng không được trống." });

            if (await _context.DanhMucKyNangs.AnyAsync(s => s.TenKyNang.ToLower() == model.TenKyNang.ToLower().Trim()))
                return Json(new { success = false, message = "Kỹ năng này đã tồn tại." });

            _context.DanhMucKyNangs.Add(new DanhMucKyNang
            {
                TenKyNang = model.TenKyNang.Trim(),
                PhanLoai  = model.PhanLoai
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm kỹ năng mới thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSkill([FromBody] UpdateSkillRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenKyNang))
                return Json(new { success = false, message = "Tên kỹ năng không được trống." });

            var skill = await _context.DanhMucKyNangs.FirstOrDefaultAsync(s => s.MaKyNang == model.MaKyNang);
            if (skill == null)
                return Json(new { success = false, message = "Không tìm thấy kỹ năng." });

            // Kiểm tra trùng tên với kỹ năng khác
            if (await _context.DanhMucKyNangs.AnyAsync(s => s.MaKyNang != model.MaKyNang && s.TenKyNang.ToLower() == model.TenKyNang.ToLower().Trim()))
                return Json(new { success = false, message = "Tên kỹ năng này đã tồn tại." });

            skill.TenKyNang = model.TenKyNang.Trim();
            skill.PhanLoai  = model.PhanLoai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật kỹ năng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSkill([FromBody] DeleteSkillRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var skill = await _context.DanhMucKyNangs.FirstOrDefaultAsync(s => s.MaKyNang == model.MaKyNang);
            if (skill == null)
                return Json(new { success = false, message = "Không tìm thấy kỹ năng cần xóa." });

            // Kiểm tra ràng buộc khóa ngoại trước khi xóa
            bool isUsed = await _context.ChiTietKyNangTinTuyenDungs.AnyAsync(c => c.MaKyNang == model.MaKyNang)
                       || await _context.ChiTietKyNangUngViens.AnyAsync(c => c.MaKyNang == model.MaKyNang);

            if (isUsed)
                return Json(new { success = false, message = "Kỹ năng này đang được sử dụng bởi ứng viên hoặc tin tuyển dụng. Không thể xóa!" });

            try
            {
                _context.DanhMucKyNangs.Remove(skill);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa kỹ năng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // BÁO CÁO THỐNG KÊ
        // =====================================================================

        public async Task<IActionResult> Reports()
        {
            int currentYear = DateTime.Now.Year;

            // 1. Thống kê lượt ứng tuyển theo tháng trong năm nay
            // Dùng GroupBy trực tiếp trên DB để tối ưu hiệu suất
            var appsByMonthRaw = await _context.DonUngTuyens
                .Where(d => d.NgayNop.HasValue && d.NgayNop.Value.Year == currentYear)
                .GroupBy(d => d.NgayNop!.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            // Khởi tạo 12 tháng mặc định = 0, sau đó điền số liệu thực tế
            var appsByMonth = Enumerable.Range(1, 12).ToDictionary(m => m, m => 0);
            foreach (var item in appsByMonthRaw)
                appsByMonth[item.Month] = item.Count;

            // 2. Phân bổ vai trò người dùng để vẽ biểu đồ hình tròn
            var roleDistribution = await _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .GroupBy(t => t.MaVaiTroNavigation.TenVaiTro)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            int totalCandidates = roleDistribution.FirstOrDefault(r => r.Role == "UngVien")?.Count ?? 0;
            int totalEmployers  = roleDistribution.FirstOrDefault(r => r.Role == "NhaTuyenDung")?.Count ?? 0;
            int totalAdmins     = roleDistribution.FirstOrDefault(r => r.Role == "Admin")?.Count ?? 0;
            int totalUsers      = totalCandidates + totalEmployers + totalAdmins;

            // Tính phần trăm để vẽ biểu đồ hình tròn trên View
            ViewBag.CandidatePct = totalUsers > 0 ? totalCandidates * 100 / totalUsers : 0;
            ViewBag.EmployerPct  = totalUsers > 0 ? totalEmployers  * 100 / totalUsers : 0;
            ViewBag.AdminPct     = totalUsers > 0 ? totalAdmins     * 100 / totalUsers : 0;

            // 3. Danh sách hoạt động gần nhất (gộp từ 3 nguồn dữ liệu)
            var recentActivities = await BuildRecentActivitiesAsync(topN: 5);

            ViewBag.AppsByMonth    = appsByMonth;
            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        /// <summary>
        /// Xuất file Excel (CSV UTF-8 có BOM) tổng hợp 150 hoạt động gần nhất.
        /// Dùng BOM (Byte Order Mark) để đảm bảo Excel đọc đúng tiếng Việt.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            // Lấy 50 bản ghi mỗi loại để xuất file tổng hợp (tổng tối đa ~150 dòng)
            var activities = await BuildActivityLogsAsync(
                queryStart: new DateTime(2000, 1, 1),
                queryEnd:   new DateTime(2100, 1, 1),
                reportType: "All",
                topN: 50
            );

            var csvBytes = BuildCsvBytes(activities);
            return File(csvBytes, "text/csv", $"BaoCaoTongHop_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        /// <summary>
        /// Tạo báo cáo tùy chỉnh theo loại dữ liệu và khoảng thời gian do Admin chọn.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExportCustomReport(string reportType, string dateRange)
        {
            // Resolve khoảng thời gian từ tham số
            var (queryStart, queryEnd) = ResolveDateRange(dateRange);

            var activities = await BuildActivityLogsAsync(queryStart, queryEnd, reportType, topN: int.MaxValue);
            var csvBytes   = BuildCsvBytes(activities);

            return File(csvBytes, "text/csv", $"CustomReport_{dateRange}_{DateTime.Now:yyyyMMdd}.csv");
        }

        // =====================================================================
        // HỒ SƠ ADMIN
        // =====================================================================

        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Admin");

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == userId);
            if (account == null) return RedirectToAction("Login", "Admin");

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
                return Json(new { success = false, message = "Email không được để trống." });

            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Không tìm thấy phiên đăng nhập." });

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == userId);
            if (account == null)
                return Json(new { success = false, message = "Tài khoản không tồn tại." });

            // Kiểm tra trùng email với tài khoản khác
            if (await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == model.Email.ToLower().Trim() && t.MaTaiKhoan != userId))
                return Json(new { success = false, message = "Email này đã được sử dụng bởi một tài khoản khác." });

            // Đổi mật khẩu nếu người dùng cung cấp mật khẩu mới
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu mới." });

                if (!_authService.VerifyPassword(model.CurrentPassword, account.MatKhauHash))
                    return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });

                account.MatKhauHash = _authService.HashPassword(model.NewPassword);
            }

            account.Email       = model.Email.Trim();
            account.NgayCapNhat = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật lại Auth Cookies để đồng bộ email mới trong Claims
                SetAuthCookies(account, "Admin", "Quản trị viên");
                return Json(new { success = true, message = "Cập nhật thông tin hồ sơ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // QUẢN LÝ VAI TRÒ
        // =====================================================================

        public async Task<IActionResult> Roles()
        {
            var roles = await _context.VaiTros.ToListAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] AddRoleRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
                return Json(new { success = false, message = "Tên vai trò không được trống." });

            if (await _context.VaiTros.AnyAsync(r => r.TenVaiTro.ToLower() == model.TenVaiTro.ToLower().Trim()))
                return Json(new { success = false, message = "Tên vai trò này đã tồn tại." });

            _context.VaiTros.Add(new VaiTro
            {
                TenVaiTro = model.TenVaiTro.Trim(),
                MoTa      = model.MoTa?.Trim()
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm vai trò mới thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDetailRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
                return Json(new { success = false, message = "Tên vai trò không được trống." });

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.MaVaiTro == model.MaVaiTro);
            if (role == null)
                return Json(new { success = false, message = "Không tìm thấy vai trò cần sửa." });

            if (await _context.VaiTros.AnyAsync(r => r.MaVaiTro != model.MaVaiTro && r.TenVaiTro.ToLower() == model.TenVaiTro.ToLower().Trim()))
                return Json(new { success = false, message = "Tên vai trò này đã tồn tại." });

            role.TenVaiTro = model.TenVaiTro.Trim();
            role.MoTa      = model.MoTa?.Trim();
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật vai trò thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.MaVaiTro == model.MaVaiTro);
            if (role == null)
                return Json(new { success = false, message = "Không tìm thấy vai trò cần xóa." });

            // Nghiệp vụ: không được xóa vai trò đang được gán cho tài khoản nào
            if (await _context.TaiKhoans.AnyAsync(t => t.MaVaiTro == model.MaVaiTro))
                return Json(new { success = false, message = "Không thể xóa vai trò này vì đang có người dùng thuộc vai trò này!" });

            try
            {
                _context.VaiTros.Remove(role);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa vai trò thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // HELPER METHODS (Private)
        // =====================================================================

        /// <summary>
        /// Lấy ID tài khoản của Admin đang đăng nhập từ Claims.
        /// Trả về 0 nếu không xác định được (chưa đăng nhập hoặc Claims không hợp lệ).
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Phân giải khoảng thời gian từ tham số string sang DateTime.
        /// Dùng giá trị an toàn (năm 2000-2100) để tránh lỗi SQL DateTime overflow.
        /// </summary>
        private static (DateTime Start, DateTime End) ResolveDateRange(string dateRange)
        {
            var now = DateTime.Now;
            return dateRange switch
            {
                "ThisMonth"  => (new DateTime(now.Year, now.Month, 1),
                                 new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
                "LastMonth"  => (new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                                 new DateTime(now.Year, now.Month, 1).AddTicks(-1)),
                // Mặc định: khoảng rất rộng để lấy tất cả dữ liệu hệ thống
                _            => (new DateTime(2000, 1, 1), new DateTime(2100, 1, 1))
            };
        }

        /// <summary>
        /// Xây dựng danh sách hoạt động gần nhất để hiển thị trên Dashboard/Reports.
        /// Gộp từ 3 nguồn: Tin tuyển dụng, Lượt ứng tuyển, Người dùng mới.
        /// </summary>
        private async Task<List<ActivityLogDto>> BuildRecentActivitiesAsync(int topN)
        {
            var (start, end) = (new DateTime(2000, 1, 1), new DateTime(2100, 1, 1));
            return await BuildActivityLogsAsync(start, end, "All", topN);
        }

        /// <summary>
        /// Xây dựng danh sách ActivityLog lọc theo loại báo cáo và khoảng thời gian.
        /// Dùng LINQ Join để tối ưu số lượng query gửi đến database.
        /// </summary>
        private async Task<List<ActivityLogDto>> BuildActivityLogsAsync(
            DateTime queryStart, DateTime queryEnd, string reportType, int topN)
        {
            var logs = new List<ActivityLogDto>();

            if (reportType is "All" or "Jobs")
            {
                var jobs = await _context.TinTuyenDungs
                    .Where(t => t.NgayTao >= queryStart && t.NgayTao <= queryEnd)
                    .OrderByDescending(t => t.NgayTao)
                    .Take(topN)
                    .Join(_context.NhaTuyenDungs,
                          t => t.MaNhaTuyenDung, n => n.MaNhaTuyenDung,
                          (t, n) => new ActivityLogDto
                          {
                              Time     = t.NgayTao,
                              Type     = "TIN TUYỂN DỤNG",
                              Content  = "Tạo mới tin: " + t.TieuDe,
                              Actor    = n.TenCongTy ?? "Nhà tuyển dụng",
                              CssClass = "el-status--info"
                          })
                    .ToListAsync();
                logs.AddRange(jobs);
            }

            if (reportType is "All" or "Apps")
            {
                var apps = await _context.DonUngTuyens
                    .Where(d => d.NgayNop >= queryStart && d.NgayNop <= queryEnd)
                    .OrderByDescending(d => d.NgayNop)
                    .Take(topN)
                    .Join(_context.UngViens,
                          d => d.MaUngVien, u => u.MaUngVien,
                          (d, u) => new ActivityLogDto
                          {
                              Time     = d.NgayNop,
                              Type     = "ỨNG TUYỂN",
                              Content  = "Ứng tuyển tin #" + d.MaTin,
                              Actor    = u.HoTen ?? "Ứng viên",
                              CssClass = "el-status--success"
                          })
                    .ToListAsync();
                logs.AddRange(apps);
            }

            if (reportType is "All" or "Users")
            {
                var users = await _context.TaiKhoans
                    .Where(t => t.NgayTao >= queryStart && t.NgayTao <= queryEnd)
                    .OrderByDescending(t => t.NgayTao)
                    .Take(topN)
                    .Join(_context.VaiTros,
                          t => t.MaVaiTro, v => v.MaVaiTro,
                          (t, v) => new ActivityLogDto
                          {
                              Time     = t.NgayTao,
                              Type     = "NGƯỜI DÙNG",
                              Content  = "Đăng ký mới (" + v.TenVaiTro + ")",
                              Actor    = t.Email,
                              CssClass = "el-status--warning"
                          })
                    .ToListAsync();
                logs.AddRange(users);
            }

            return logs.OrderByDescending(x => x.Time).Take(topN == int.MaxValue ? int.MaxValue : topN * 3).ToList();
        }

        /// <summary>
        /// Chuyển đổi danh sách ActivityLog thành mảng byte CSV UTF-8 có BOM.
        /// BOM (EF BB BF) cần thiết để Excel tự nhận dạng encoding UTF-8 và hiển thị tiếng Việt đúng.
        /// </summary>
        private static byte[] BuildCsvBytes(IEnumerable<ActivityLogDto> activities)
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Thời gian,Loại dữ liệu,Nội dung,Người thực hiện");

            foreach (var item in activities)
            {
                // Escape dấu ngoặc kép trong nội dung để chuẩn định dạng CSV (RFC 4180)
                var content = item.Content.Replace("\"", "\"\"");
                var actor   = item.Actor.Replace("\"", "\"\"");
                builder.AppendLine($"{item.Time:dd/MM/yyyy HH:mm},{item.Type},\"{content}\",\"{actor}\"");
            }

            // Thêm BOM vào đầu file để Excel nhận dạng UTF-8
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            return bom.Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }

        /// <summary>
        /// Cập nhật lại JWT cookies sau khi thay đổi thông tin tài khoản.
        /// Đảm bảo Claims trong cookie được đồng bộ với dữ liệu mới nhất.
        /// </summary>
        private void SetAuthCookies(TaiKhoan account, string roleName, string fullName)
        {
            var accessToken  = _authService.GenerateAccessToken(account, roleName, fullName);
            var refreshToken = _authService.GenerateRefreshToken(account);

            var cookieOptions = new CookieOptions
            {
                HttpOnly  = true,
                Secure    = true,              // Chỉ gửi qua HTTPS
                SameSite  = SameSiteMode.Strict,
                Expires   = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("AccessToken",  accessToken,  cookieOptions);
            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }
    }
}
