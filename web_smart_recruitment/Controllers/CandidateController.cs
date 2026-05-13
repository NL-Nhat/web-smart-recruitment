using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;
using System.IO;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "UngVien")]
    public class CandidateController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CandidateController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Chức năng nộp đơn ứng tuyển cho Ứng viên
        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> Apply(int maTin, IFormFile cvFile)
        {
            // 1. Kiểm tra file CV đầu vào
            if (cvFile == null || cvFile.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file CV của bạn." });
            }

            // 2. Lấy ID người dùng hiện tại từ Claims
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại." });
            }

            try
            {
                // 3. Xử lý lưu file PDF vào thư mục wwwroot/uploads/cvs
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file duy nhất để tránh bị ghi đè
                string uniqueFileName = $"{Guid.NewGuid()}_{cvFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(fileStream);
                }

                // 4. Lưu thông tin hồ sơ CV vào Database
                var hoSo = new HoSoCv
                {
                    MaUngVien = userId,
                    TenFile = cvFile.FileName,
                    DuongDanFile = "/uploads/cvs/" + uniqueFileName,
                    DinhDang = Path.GetExtension(cvFile.FileName).ToLower(),
                    NgayTaiLen = DateTime.Now
                };
                _context.HoSoCvs.Add(hoSo);
                await _context.SaveChangesAsync(); // Lưu để lấy MaCv vừa tạo

                // 5. Tạo đơn ứng tuyển mới
                var donUngTuyen = new DonUngTuyen
                {
                    MaTin = maTin,
                    MaUngVien = userId,
                    MaCv = hoSo.MaCv,
                    NgayNop = DateTime.Now,
                    TrangThai = "DaNop",
                    NgayCapNhat = DateTime.Now
                };
                _context.DonUngTuyens.Add(donUngTuyen);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Nộp đơn thành công! Nhà tuyển dụng sẽ nhận được hồ sơ của bạn." });
            }
            catch (Exception ex)
            {
                // Trả về thông báo lỗi nếu có sự cố hệ thống
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult Jobs(string q, string loc)
        {
            var query = _context.TinTuyenDungs
                .Include(t => t.MaNhaTuyenDungNavigation)
                .Include(t => t.ChiTietKyNangTinTuyenDungs)
                    .ThenInclude(c => c.MaKyNangNavigation)
                .Where(t => t.TrangThai == "DangMo" && (t.DaXoa == false || t.DaXoa == null));

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(t => t.TieuDe.Contains(q) || t.MoTaCongViec.Contains(q) || t.MaNhaTuyenDungNavigation.TenCongTy.Contains(q));
            }

            if (!string.IsNullOrEmpty(loc))
            {
                query = query.Where(t => t.DiaDiem.Contains(loc));
            }

            var jobs = query.OrderByDescending(t => t.NgayTao).ToList();
            return View(jobs);
        }

        [AllowAnonymous]
        public IActionResult JobDetail(int maTin)
        {
            var job = _context.TinTuyenDungs
                .Include(t => t.MaNhaTuyenDungNavigation)
                .Include(t => t.ChiTietKyNangTinTuyenDungs)
                    .ThenInclude(c => c.MaKyNangNavigation)
                .FirstOrDefault(t => t.MaTin == maTin);

            if (job == null) return NotFound();

            // Kiểm tra xem ứng viên hiện tại đã nộp đơn cho tin này chưa
            bool hasApplied = false;
            if (User.Identity.IsAuthenticated && User.IsInRole("UngVien"))
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int userId))
                {
                    hasApplied = _context.DonUngTuyens.Any(d => d.MaTin == maTin && d.MaUngVien == userId);
                }
            }
            ViewBag.HasApplied = hasApplied;

            return View(job);
        }

        [Authorize(Roles = "UngVien")]
        public IActionResult Applications() => View();
        
        [Authorize(Roles = "UngVien")]
        public IActionResult Interviews() => View();
        
        [Authorize(Roles = "UngVien")]
        public IActionResult Profile() => View();
        
        [AllowAnonymous]
        public IActionResult CompanyDetail() => View();
    }
}
