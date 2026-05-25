using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.ViewModels;
using web_smart_recruitment.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace web_smart_recruitment.Controllers
{
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
        public IActionResult Dashboard() => View();

        public async Task<IActionResult> Users()
        {
            var users = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .Select(a => new AdminUserViewModel
                {
                    MaTaiKhoan = a.MaTaiKhoan,
                    Email = a.Email,
                    TenHienThi = a.MaVaiTroNavigation.TenVaiTro == "UngVien" 
                        ? (_context.UngViens.FirstOrDefault(u => u.MaUngVien == a.MaTaiKhoan) != null ? _context.UngViens.FirstOrDefault(u => u.MaUngVien == a.MaTaiKhoan).HoTen : "Chưa cập nhật")
                        : (a.MaVaiTroNavigation.TenVaiTro == "NhaTuyenDung"
                            ? (_context.NhaTuyenDungs.FirstOrDefault(n => n.MaNhaTuyenDung == a.MaTaiKhoan) != null ? _context.NhaTuyenDungs.FirstOrDefault(n => n.MaNhaTuyenDung == a.MaTaiKhoan).TenCongTy ?? "Chưa cập nhật" : "Chưa cập nhật")
                            : "Quản trị viên"),
                    TenVaiTro = a.MaVaiTroNavigation.TenVaiTro,
                    TrangThaiHoatDong = a.TrangThaiHoatDong ?? true
                })
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Email và mật khẩu không được trống." });
            }

            var exists = await _context.TaiKhoans.AnyAsync(a => a.Email == model.Email);
            if (exists)
            {
                return Json(new { success = false, message = "Email này đã tồn tại trong hệ thống." });
            }

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == model.VaiTro);
            if (role == null)
            {
                return Json(new { success = false, message = "Vai trò không hợp lệ." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newAccount = new TaiKhoan
                {
                    Email = model.Email,
                    MatKhauHash = _authService.HashPassword(model.Password),
                    MaVaiTro = role.MaVaiTro,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };

                _context.TaiKhoans.Add(newAccount);
                await _context.SaveChangesAsync();

                if (model.VaiTro == "UngVien")
                {
                    var ungVien = new UngVien
                    {
                        MaUngVien = newAccount.MaTaiKhoan,
                        HoTen = model.HoTenCandidate ?? "Ứng viên mới",
                        SoDienThoai = model.SoDienThoaiCandidate
                    };
                    _context.UngViens.Add(ungVien);
                }
                else if (model.VaiTro == "NhaTuyenDung")
                {
                    var ntd = new NhaTuyenDung
                    {
                        MaNhaTuyenDung = newAccount.MaTaiKhoan,
                        TenCongTy = model.TenCongTy ?? "Công ty mới",
                        SoDienThoai = model.SoDienThoaiEmployer,
                        DiaChi = model.DiaChi
                    };
                    _context.NhaTuyenDungs.Add(ntd);
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

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateRoleModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var account = await _context.TaiKhoans
                .Include(a => a.MaVaiTroNavigation)
                .FirstOrDefaultAsync(a => a.MaTaiKhoan == model.MaTaiKhoan);

            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            var newRole = await _context.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == model.TenVaiTro);
            if (newRole == null)
            {
                return Json(new { success = false, message = "Vai trò mới không hợp lệ." });
            }

            if (account.MaVaiTro == newRole.MaVaiTro)
            {
                return Json(new { success = true, message = "Vai trò không thay đổi." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                account.MaVaiTro = newRole.MaVaiTro;
                account.NgayCapNhat = DateTime.Now;

                if (model.TenVaiTro == "UngVien")
                {
                    var exists = await _context.UngViens.AnyAsync(u => u.MaUngVien == account.MaTaiKhoan);
                    if (!exists)
                    {
                        var newUngVien = new UngVien
                        {
                            MaUngVien = account.MaTaiKhoan,
                            HoTen = "Ứng viên mới",
                            SoDienThoai = ""
                        };
                        _context.UngViens.Add(newUngVien);
                    }
                }
                else if (model.TenVaiTro == "NhaTuyenDung")
                {
                    var exists = await _context.NhaTuyenDungs.AnyAsync(n => n.MaNhaTuyenDung == account.MaTaiKhoan);
                    if (!exists)
                    {
                        var newNtd = new NhaTuyenDung
                        {
                            MaNhaTuyenDung = account.MaTaiKhoan,
                            TenCongTy = "Công ty mới",
                            SoDienThoai = "",
                            DiaChi = ""
                        };
                        _context.NhaTuyenDungs.Add(newNtd);
                    }
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

        public class UpdateRoleModel
        {
            public int MaTaiKhoan { get; set; }
            public string TenVaiTro { get; set; } = null!;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleStatusModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(a => a.MaTaiKhoan == model.MaTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            bool currentStatus = account.TrangThaiHoatDong ?? true;
            account.TrangThaiHoatDong = !currentStatus;
            account.NgayCapNhat = DateTime.Now;

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

        public class ToggleStatusModel
        {
            public int MaTaiKhoan { get; set; }
        }

        public async Task<IActionResult> Skills()
        {
            var skills = await _context.DanhMucKyNangs.ToListAsync();
            return View(skills);
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenKyNang))
            {
                return Json(new { success = false, message = "Tên kỹ năng không được trống." });
            }

            var exists = await _context.DanhMucKyNangs.AnyAsync(s => s.TenKyNang.ToLower() == model.TenKyNang.ToLower().Trim());
            if (exists)
            {
                return Json(new { success = false, message = "Kỹ năng này đã tồn tại." });
            }

            var newSkill = new DanhMucKyNang
            {
                TenKyNang = model.TenKyNang.Trim(),
                PhanLoai = model.PhanLoai
            };

            _context.DanhMucKyNangs.Add(newSkill);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm kỹ năng mới thành công!" });
        }

        public class AddSkillModel
        {
            public string TenKyNang { get; set; } = null!;
            public string? PhanLoai { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSkill([FromBody] UpdateSkillModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenKyNang))
            {
                return Json(new { success = false, message = "Tên kỹ năng không được trống." });
            }

            var skill = await _context.DanhMucKyNangs.FirstOrDefaultAsync(s => s.MaKyNang == model.MaKyNang);
            if (skill == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kỹ năng." });
            }

            var exists = await _context.DanhMucKyNangs.AnyAsync(s => s.MaKyNang != model.MaKyNang && s.TenKyNang.ToLower() == model.TenKyNang.ToLower().Trim());
            if (exists)
            {
                return Json(new { success = false, message = "Tên kỹ năng này đã tồn tại." });
            }

            skill.TenKyNang = model.TenKyNang.Trim();
            skill.PhanLoai = model.PhanLoai;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật kỹ năng thành công!" });
        }

        public class UpdateSkillModel
        {
            public int MaKyNang { get; set; }
            public string TenKyNang { get; set; } = null!;
            public string? PhanLoai { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSkill([FromBody] DeleteSkillModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var skill = await _context.DanhMucKyNangs.FirstOrDefaultAsync(s => s.MaKyNang == model.MaKyNang);
            if (skill == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kỹ năng cần xóa." });
            }

            // Kiểm tra ràng buộc khóa ngoại trước khi xóa
            var isUsedInJob = await _context.ChiTietKyNangTinTuyenDungs.AnyAsync(c => c.MaKyNang == model.MaKyNang);
            var isUsedInCandidate = await _context.ChiTietKyNangUngViens.AnyAsync(c => c.MaKyNang == model.MaKyNang);

            if (isUsedInJob || isUsedInCandidate)
            {
                return Json(new { success = false, message = "Kỹ năng này đang được sử dụng bởi ứng viên hoặc tin tuyển dụng. Không thể xóa!" });
            }

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

        public class DeleteSkillModel
        {
            public int MaKyNang { get; set; }
        }

        public IActionResult Reports() => View();
        
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Admin");
            }

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == userId);
            if (account == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, message = "Email không được để trống." });
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Không tìm thấy phiên đăng nhập." });
            }

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == userId);
            if (account == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            // Kiểm tra trùng email với tài khoản khác
            var exists = await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == model.Email.ToLower().Trim() && t.MaTaiKhoan != userId);
            if (exists)
            {
                return Json(new { success = false, message = "Email này đã được sử dụng bởi một tài khoản khác." });
            }

            // Cập nhật mật khẩu nếu được cung cấp mật khẩu mới
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu mới." });
                }

                if (!_authService.VerifyPassword(model.CurrentPassword, account.MatKhauHash))
                {
                    return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác." });
                }

                account.MatKhauHash = _authService.HashPassword(model.NewPassword);
            }

            account.Email = model.Email.Trim();
            account.NgayCapNhat = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật lại Auth Cookies để đồng bộ Email mới của Admin
                SetAuthCookies(account, "Admin", "Quản trị viên");
                return Json(new { success = true, message = "Cập nhật thông tin hồ sơ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        
        public async Task<IActionResult> Roles()
        {
            var roles = await _context.VaiTros.ToListAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
            {
                return Json(new { success = false, message = "Tên vai trò không được trống." });
            }

            var exists = await _context.VaiTros.AnyAsync(r => r.TenVaiTro.ToLower() == model.TenVaiTro.ToLower().Trim());
            if (exists)
            {
                return Json(new { success = false, message = "Tên vai trò này đã tồn tại." });
            }

            var newRole = new VaiTro
            {
                TenVaiTro = model.TenVaiTro.Trim(),
                MoTa = model.MoTa?.Trim()
            };

            _context.VaiTros.Add(newRole);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm vai trò mới thành công!" });
        }

        public class AddRoleModel
        {
            public string TenVaiTro { get; set; } = null!;
            public string? MoTa { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDetailModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenVaiTro))
            {
                return Json(new { success = false, message = "Tên vai trò không được trống." });
            }

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.MaVaiTro == model.MaVaiTro);
            if (role == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vai trò cần sửa." });
            }

            var exists = await _context.VaiTros.AnyAsync(r => r.MaVaiTro != model.MaVaiTro && r.TenVaiTro.ToLower() == model.TenVaiTro.ToLower().Trim());
            if (exists)
            {
                return Json(new { success = false, message = "Tên vai trò này đã tồn tại." });
            }

            role.TenVaiTro = model.TenVaiTro.Trim();
            role.MoTa = model.MoTa?.Trim();

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật vai trò thành công!" });
        }

        public class UpdateRoleDetailModel
        {
            public int MaVaiTro { get; set; }
            public string TenVaiTro { get; set; } = null!;
            public string? MoTa { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var role = await _context.VaiTros.FirstOrDefaultAsync(r => r.MaVaiTro == model.MaVaiTro);
            if (role == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vai trò cần xóa." });
            }

            // Nghiệp vụ: Chỉ được xóa khi chưa có ai có vai trò đó
            var isAssigned = await _context.TaiKhoans.AnyAsync(t => t.MaVaiTro == model.MaVaiTro);
            if (isAssigned)
            {
                return Json(new { success = false, message = "Không thể xóa vai trò này vì đang có người dùng thuộc vai trò này!" });
            }

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

        private void SetAuthCookies(TaiKhoan account, string roleName, string fullName)
        {
            var accessToken = _authService.GenerateAccessToken(account, roleName, fullName);
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

        public class UpdateProfileModel
        {
            public string Email { get; set; } = null!;
            public string? CurrentPassword { get; set; }
            public string? NewPassword { get; set; }
        }

        public class DeleteRoleModel
        {
            public int MaVaiTro { get; set; }
        }

        public class AddUserModel
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string VaiTro { get; set; } = null!;
            
            // Candidate info
            public string? HoTenCandidate { get; set; }
            public string? SoDienThoaiCandidate { get; set; }
            
            // Employer info
            public string? TenCongTy { get; set; }
            public string? SoDienThoaiEmployer { get; set; }
            public string? DiaChi { get; set; }
        }
    }
}
