using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Skills() => View();
        public IActionResult Reports() => View();
        public IActionResult Profile() => View();
        public IActionResult Roles() => View();

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
