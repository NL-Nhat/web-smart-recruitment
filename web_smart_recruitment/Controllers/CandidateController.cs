using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;
using web_smart_recruitment.Services;
using web_smart_recruitment.Models.ViewModels;
using System.IO;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "UngVien")]
    public class CandidateController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        // Hàng đợi xử lý CV ngầm - inject từ DI Container
        private readonly CvAnalysisQueue _cvQueue;

        public CandidateController(
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment,
            CvAnalysisQueue cvQueue)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _cvQueue = cvQueue;
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

                // 4. Tạo đơn ứng tuyển mới và lưu thông tin CV
                var donUngTuyen = new DonUngTuyen
                {
                    MaTin = maTin,
                    MaUngVien = userId,
                    TenFile = cvFile.FileName,
                    DuongDanFile = "/uploads/cvs/" + uniqueFileName,
                    DinhDang = Path.GetExtension(cvFile.FileName).ToLower(),
                    NgayNop = DateTime.Now,
                    TrangThai = "DaNop",
                    NgayCapNhat = DateTime.Now
                };
                _context.DonUngTuyens.Add(donUngTuyen);
                await _context.SaveChangesAsync();

                // ============================================================
                // KÍCH HOẠT PHÂN TÍCH CV NGẦM (Background Processing)
                //
                // Bước 6a: Tạo bản ghi KetQua_AI với trạng thái "DangXuLy"
                // để HR có thể thấy đơn đang được AI xử lý
                // ============================================================
                _context.KetQuaAis.Add(new KetQuaAi
                {
                    MaDon         = donUngTuyen.MaDon,
                    TrangThaiXuLy = "DangXuLy",  // Trạng thái khởi tạo
                    NgayPhanTich  = DateTime.Now
                });
                await _context.SaveChangesAsync();

                // Bước 6b: Đẩy MaDon vào hàng đợi để Background Service xử lý ngầm
                // Controller trả về response ngay, người dùng không cần chờ AI phân tích
                await _cvQueue.EnqueueAsync(donUngTuyen.MaDon);

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
        public async Task<IActionResult> Applications()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                    .ThenInclude(t => t.MaNhaTuyenDungNavigation)
                .Include(d => d.KetQuaAis)
                .Where(d => d.MaUngVien == userId);

            var applications = await query
                .OrderByDescending(d => d.NgayNop)
                .Select(d => new ApplicationsViewModel
                {
                    MaDon = d.MaDon,
                    MaTin = d.MaTin ?? 0,
                    TieuDeCongViec = d.MaTinNavigation != null ? d.MaTinNavigation.TieuDe : "Cong viec da bi xoa",
                    TenCongTy = d.MaTinNavigation != null && d.MaTinNavigation.MaNhaTuyenDungNavigation != null 
                        ? d.MaTinNavigation.MaNhaTuyenDungNavigation.TenCongTy ?? "Nha tuyen dung" 
                        : "Cong ty an danh",
                    LogoCongTy = d.MaTinNavigation != null && d.MaTinNavigation.MaNhaTuyenDungNavigation != null 
                        ? d.MaTinNavigation.MaNhaTuyenDungNavigation.Logo 
                        : null,
                    NgayNop = d.NgayNop ?? DateTime.Now,
                    TrangThai = d.TrangThai ?? "DaNop",
                    DiemPhuHop = d.KetQuaAis.FirstOrDefault() != null ? d.KetQuaAis.FirstOrDefault().DiemPhuHop : null,
                    TrangThaiAI = d.KetQuaAis.FirstOrDefault() != null ? d.KetQuaAis.FirstOrDefault().TrangThaiXuLy : null
                })
                .ToListAsync();

            return View(applications);
        }
        
        [Authorize(Roles = "UngVien")]
        public IActionResult Interviews()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation)
                    .ThenInclude(d => d.MaTinNavigation)
                        .ThenInclude(t => t.MaNhaTuyenDungNavigation)
                .Where(l => l.MaDonNavigation.MaUngVien == userId);

            var interviews = query
                .OrderBy(l => l.NgayPhuongVan)
                .ThenBy(l => l.GioPhuongVan)
                .ToList();

            // Tinh so buoi phong van trong tuan nay
            var today = DateOnly.FromDateTime(DateTime.Today);
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-1 * diff);
            var endOfWeek = startOfWeek.AddDays(7);

            ViewBag.InterviewsThisWeek = interviews.Count(l => l.NgayPhuongVan >= startOfWeek && l.NgayPhuongVan < endOfWeek && l.TrangThai != "DaHuy");

            return View(interviews);
        }

        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> AcceptInterview(int maLichHen)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            var interview = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == maLichHen && l.MaDonNavigation.MaUngVien == userId);

            if (interview == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch phỏng vấn này." });
            }

            if (interview.TrangThai != "ChoXacNhan")
            {
                return Json(new { success = false, message = "Lịch phỏng vấn này không thể xác nhận ở trạng thái hiện tại." });
            }

            interview.TrangThai = "DaXacNhan";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xác nhận chấp nhận lịch hẹn phỏng vấn!" });
        }

        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> DeclineInterview(int maLichHen)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            var interview = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == maLichHen && l.MaDonNavigation.MaUngVien == userId);

            if (interview == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch phỏng vấn này." });
            }

            if (interview.TrangThai != "ChoXacNhan")
            {
                return Json(new { success = false, message = "Lịch phỏng vấn này không thể từ chối ở trạng thái hiện tại." });
            }

            interview.TrangThai = "DaHuy";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xác nhận từ chối lịch hẹn phỏng vấn!" });
        }
        
        [Authorize(Roles = "UngVien")]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var candidate = await _context.UngViens
                    .Include(u => u.ChiTietKyNangUngViens)
                        .ThenInclude(ck => ck.MaKyNangNavigation)
                    .FirstOrDefaultAsync(u => u.MaUngVien == userId);
                
                if (candidate != null)
                {
                    return View(candidate);
                }
            }
            return RedirectToAction("Login", "Auth");
        }

        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại." });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.HoTen))
            {
                return Json(new { success = false, message = "Họ tên không được để trống." });
            }

            try
            {
                var candidate = await _context.UngViens.FirstOrDefaultAsync(u => u.MaUngVien == userId);
                if (candidate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin ứng viên." });
                }

                candidate.HoTen = model.HoTen;
                candidate.SoDienThoai = model.SoDienThoai;
                candidate.SoNamKinhNghiem = model.SoNamKinhNghiem ?? 0;
                candidate.LinkLinkedIn = model.LinkLinkedIn;
                candidate.ChucDanhHienTai = model.ChucDanhHienTai;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật hồ sơ cá nhân thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [Authorize(Roles = "UngVien")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableSkills()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            try
            {
                var existingSkillIds = await _context.ChiTietKyNangUngViens
                    .Where(c => c.MaUngVien == userId)
                    .Select(c => c.MaKyNang)
                    .ToListAsync();

                var availableSkills = await _context.DanhMucKyNangs
                    .Where(s => !existingSkillIds.Contains(s.MaKyNang))
                    .Select(s => new { s.MaKyNang, s.TenKyNang })
                    .ToListAsync();

                return Json(new { success = true, skills = availableSkills });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillModel model)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            try
            {
                var exists = await _context.ChiTietKyNangUngViens
                    .AnyAsync(c => c.MaUngVien == userId && c.MaKyNang == model.MaKyNang);

                if (exists)
                {
                    return Json(new { success = false, message = "Kỹ năng này đã tồn tại trong hồ sơ của bạn." });
                }

                var skillDetail = new ChiTietKyNangUngVien
                {
                    MaUngVien = userId,
                    MaKyNang = model.MaKyNang,
                    SoNamKinhNghiem = 0
                };

                _context.ChiTietKyNangUngViens.Add(skillDetail);
                await _context.SaveChangesAsync();

                var skillName = await _context.DanhMucKyNangs
                    .Where(s => s.MaKyNang == model.MaKyNang)
                    .Select(s => s.TenKyNang)
                    .FirstOrDefaultAsync();

                return Json(new { success = true, message = "Thêm kỹ năng thành công!", skillId = model.MaKyNang, skillName = skillName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Authorize(Roles = "UngVien")]
        [HttpPost]
        public async Task<IActionResult> DeleteSkill([FromBody] DeleteSkillModel model)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
            }

            try
            {
                var skillDetail = await _context.ChiTietKyNangUngViens
                    .FirstOrDefaultAsync(c => c.MaUngVien == userId && c.MaKyNang == model.MaKyNang);

                if (skillDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy kỹ năng này." });
                }

                _context.ChiTietKyNangUngViens.Remove(skillDetail);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa kỹ năng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public class UpdateProfileModel
        {
            public string HoTen { get; set; } = null!;
            public string? SoDienThoai { get; set; }
            public int? SoNamKinhNghiem { get; set; }
            public string? LinkLinkedIn { get; set; }
            public string? ChucDanhHienTai { get; set; }
        }

        public class AddSkillModel
        {
            public int MaKyNang { get; set; }
        }

        public class DeleteSkillModel
        {
            public int MaKyNang { get; set; }
        }
        
        [AllowAnonymous]
        public IActionResult CompanyDetail(int maNhaTuyenDung)
        {
            var company = _context.NhaTuyenDungs
                .FirstOrDefault(n => n.MaNhaTuyenDung == maNhaTuyenDung);

            if (company == null)
            {
                return NotFound();
            }

            var activeJobs = _context.TinTuyenDungs
                .Where(t => t.MaNhaTuyenDung == maNhaTuyenDung && t.TrangThai == "DangMo" && (t.DaXoa == false || t.DaXoa == null))
                .OrderByDescending(t => t.NgayTao)
                .ToList();

            ViewBag.ActiveJobs = activeJobs;

            return View(company);
        }
    }
}
