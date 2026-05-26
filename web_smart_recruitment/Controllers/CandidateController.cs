using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.Dtos;
using web_smart_recruitment.Models.RequestModels.Candidate;
using web_smart_recruitment.Models.ViewModels;
using web_smart_recruitment.Services;
using web_smart_recruitment.Enums;

namespace web_smart_recruitment.Controllers
{
    /// <summary>
    /// Controller cho Ứng viên:
    /// Tìm việc, nộp đơn, xem lịch phỏng vấn, quản lý hồ sơ cá nhân và kỹ năng.
    /// </summary>
    [Authorize(Roles = "UngVien")]
    public class CandidateController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        // Hàng đợi xử lý CV ngầm — inject từ DI Container (xem CvAnalysisBackgroundService)
        private readonly CvAnalysisQueue _cvQueue;

        public CandidateController(
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment,
            CvAnalysisQueue cvQueue)
        {
            _context            = context;
            _webHostEnvironment = webHostEnvironment;
            _cvQueue            = cvQueue;
        }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        /// <summary>
        /// Lấy ID tài khoản của ứng viên đang đăng nhập từ JWT Claims.
        /// Trả về 0 nếu chưa đăng nhập hoặc Claims không hợp lệ.
        /// </summary>
        private int GetCurrentUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        // =====================================================================
        // TÌM VIỆC LÀM (KHÔNG YÊU CẦU ĐĂNG NHẬP)
        // =====================================================================

        /// <summary>
        /// Trang tìm kiếm việc làm công khai.
        /// Hỗ trợ tìm theo từ khóa (q) và địa điểm (loc).
        /// </summary>
        [AllowAnonymous]
        public IActionResult Jobs(string q, string loc)
        {
            var query = _context.TinTuyenDungs
                .Include(t => t.MaNhaTuyenDungNavigation)
                .Include(t => t.ChiTietKyNangTinTuyenDungs)
                    .ThenInclude(c => c.MaKyNangNavigation)
                .Where(t => t.TrangThai == TrangThaiTin.DangMo && (t.DaXoa == false || t.DaXoa == null));

            if (!string.IsNullOrEmpty(q))
                query = query.Where(t => t.TieuDe.Contains(q)
                                      || t.MoTaCongViec.Contains(q)
                                      || t.MaNhaTuyenDungNavigation.TenCongTy.Contains(q));

            if (!string.IsNullOrEmpty(loc))
                query = query.Where(t => t.DiaDiem.Contains(loc));

            return View(query.OrderByDescending(t => t.NgayTao).ToList());
        }

        /// <summary>
        /// Chi tiết tin tuyển dụng — hiển thị công khai.
        /// Kiểm tra xem ứng viên đang đăng nhập đã ứng tuyển tin này chưa.
        /// </summary>
        [AllowAnonymous]
        public IActionResult JobDetail(int maTin)
        {
            var job = _context.TinTuyenDungs
                .Include(t => t.MaNhaTuyenDungNavigation)
                .Include(t => t.ChiTietKyNangTinTuyenDungs)
                    .ThenInclude(c => c.MaKyNangNavigation)
                .FirstOrDefault(t => t.MaTin == maTin);

            if (job == null) return NotFound();

            // Kiểm tra xem ứng viên đã nộp đơn cho tin này chưa
            bool hasApplied = false;
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("UngVien"))
            {
                int userId = GetCurrentUserId();
                hasApplied = userId > 0 && _context.DonUngTuyens.Any(d => d.MaTin == maTin && d.MaUngVien == userId);
            }
            ViewBag.HasApplied = hasApplied;

            return View(job);
        }

        /// <summary>
        /// Trang thông tin chi tiết của nhà tuyển dụng và các tin đang tuyển của họ.
        /// </summary>
        [AllowAnonymous]
        public IActionResult CompanyDetail(int maNhaTuyenDung)
        {
            var company = _context.NhaTuyenDungs.FirstOrDefault(n => n.MaNhaTuyenDung == maNhaTuyenDung);
            if (company == null) return NotFound();

            ViewBag.ActiveJobs = _context.TinTuyenDungs
                .Where(t => t.MaNhaTuyenDung == maNhaTuyenDung
                         && t.TrangThai == TrangThaiTin.DangMo
                         && (t.DaXoa == false || t.DaXoa == null))
                .OrderByDescending(t => t.NgayTao)
                .ToList();

            return View(company);
        }

        // =====================================================================
        // NỘP ĐƠN ỨNG TUYỂN
        // =====================================================================

        /// <summary>
        /// Nộp đơn ứng tuyển kèm file CV (PDF).
        ///
        /// Quy trình:
        /// 1. Lưu file CV lên server
        /// 2. Tạo đơn ứng tuyển (DonUngTuyen) với trạng thái DaNop
        /// 3. Tạo bản ghi KetQua_AI với trạng thái DangXuLy (placeholder cho AI)
        /// 4. Đẩy maDon vào hàng đợi — Background Service sẽ tự động phân tích
        ///
        /// Controller trả về ngay lập tức, ứng viên không cần chờ AI xong.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Apply(int maTin, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file CV của bạn." });

            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại." });

            try
            {
                // Lưu file CV vào thư mục wwwroot/uploads/cvs
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tên file duy nhất (GUID) để tránh bị ghi đè khi trùng tên
                string uniqueFileName = $"{Guid.NewGuid()}_{cvFile.FileName}";
                string filePath       = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await cvFile.CopyToAsync(fileStream);

                // Tạo đơn ứng tuyển với trạng thái ban đầu là DaNop
                var donUngTuyen = new DonUngTuyen
                {
                    MaTin         = maTin,
                    MaUngVien     = userId,
                    TenFile       = cvFile.FileName,
                    DuongDanFile  = "/uploads/cvs/" + uniqueFileName,
                    DinhDang      = Path.GetExtension(cvFile.FileName).ToLower(),
                    NgayNop       = DateTime.Now,
                    TrangThai     = TrangThaiDon.DaNop,
                    NgayCapNhat   = DateTime.Now
                };
                _context.DonUngTuyens.Add(donUngTuyen);
                await _context.SaveChangesAsync();

                // ============================================================
                // KÍCH HOẠT PHÂN TÍCH CV NGẦM (Background Processing)
                //
                // Tạo bản ghi KetQua_AI với trạng thái DangXuLy để HR biết
                // đơn đang được AI xử lý. Background Service sẽ cập nhật
                // trạng thái thành HoanThanh hoặc Loi sau khi xong.
                // ============================================================
                _context.KetQuaAis.Add(new KetQuaAi
                {
                    MaDon         = donUngTuyen.MaDon,
                    TrangThaiXuLy = TrangThaiKetQuaAi.DangXuLy,
                    NgayPhanTich  = DateTime.Now
                });
                await _context.SaveChangesAsync();

                // Đẩy MaDon vào hàng đợi để Background Service xử lý ngầm
                // (không block luồng hiện tại)
                await _cvQueue.EnqueueAsync(donUngTuyen.MaDon);

                return Json(new { success = true, message = "Nộp đơn thành công! Nhà tuyển dụng sẽ nhận được hồ sơ của bạn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // THEO DÕI ĐƠN ỨNG TUYỂN
        // =====================================================================

        /// <summary>
        /// Danh sách tất cả đơn ứng tuyển của ứng viên, kèm điểm AI.
        /// </summary>
        public async Task<IActionResult> Applications()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var applications = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation).ThenInclude(t => t.MaNhaTuyenDungNavigation)
                .Include(d => d.KetQuaAis)
                .Where(d => d.MaUngVien == userId)
                .OrderByDescending(d => d.NgayNop)
                .Select(d => new ApplicationsViewModel
                {
                    MaDon            = d.MaDon,
                    MaTin            = d.MaTin ?? 0,
                    TieuDeCongViec   = d.MaTinNavigation != null ? d.MaTinNavigation.TieuDe : "Công việc đã bị xóa",
                    TenCongTy        = d.MaTinNavigation != null && d.MaTinNavigation.MaNhaTuyenDungNavigation != null
                                        ? d.MaTinNavigation.MaNhaTuyenDungNavigation.TenCongTy ?? "Nhà tuyển dụng"
                                        : "Công ty ẩn danh",
                    LogoCongTy       = d.MaTinNavigation != null && d.MaTinNavigation.MaNhaTuyenDungNavigation != null
                                        ? d.MaTinNavigation.MaNhaTuyenDungNavigation.Logo
                                        : null,
                    NgayNop          = d.NgayNop ?? DateTime.Now,
                    TrangThai        = d.TrangThai ?? TrangThaiDon.DaNop,
                    DiemPhuHop       = d.KetQuaAis.FirstOrDefault() != null ? d.KetQuaAis.FirstOrDefault()!.DiemPhuHop : null,
                    TrangThaiAI      = d.KetQuaAis.FirstOrDefault() != null ? d.KetQuaAis.FirstOrDefault()!.TrangThaiXuLy : null
                })
                .ToListAsync();

            return View(applications);
        }

        // =====================================================================
        // LỊCH HẸN PHỎNG VẤN
        // =====================================================================

        /// <summary>
        /// Danh sách lịch hẹn phỏng vấn của ứng viên, sắp xếp theo ngày gần nhất.
        /// Tính thêm số buổi phỏng vấn trong tuần này để hiển thị thống kê.
        /// </summary>
        public IActionResult Interviews()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var interviews = _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaTinNavigation)
                    .ThenInclude(t => t!.MaNhaTuyenDungNavigation)
                .Where(l => l.MaDonNavigation.MaUngVien == userId)
                .OrderBy(l => l.NgayPhuongVan)
                .ThenBy(l => l.GioPhuongVan)
                .ToList();

            // Tính phạm vi tuần hiện tại (Thứ Hai đến Chủ Nhật)
            var today        = DateOnly.FromDateTime(DateTime.Today);
            int daysSinceMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek  = today.AddDays(-daysSinceMonday);
            var endOfWeek    = startOfWeek.AddDays(7);

            ViewBag.InterviewsThisWeek = interviews.Count(l =>
                l.NgayPhuongVan >= startOfWeek
             && l.NgayPhuongVan < endOfWeek
             && l.TrangThai != TrangThaiLichHen.DaHuy);

            return View(interviews);
        }

        // =====================================================================
        // XEM CHI TIẾT ĐÁNH GIÁ AI
        // =====================================================================

        /// <summary>
        /// Xem chi tiết kết quả phân tích AI cho đơn ứng tuyển của chính ứng viên.
        /// </summary>
        public async Task<IActionResult> AiCandidate(int maDon)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var application = await _context.DonUngTuyens
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.MaTinNavigation)
                .Include(d => d.KetQuaAis)
                .Include(d => d.LichHenPhongVans)
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaUngVien == userId);

            if (application == null) return NotFound();

            return View(application);
        }

        /// <summary>
        /// Ứng viên xác nhận tham gia lịch hẹn phỏng vấn.
        /// Chỉ cho phép xác nhận khi lịch đang ở trạng thái ChoXacNhan.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AcceptInterview(int maLichHen)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            var interview = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == maLichHen && l.MaDonNavigation.MaUngVien == userId);

            if (interview == null)
                return Json(new { success = false, message = "Không tìm thấy lịch phỏng vấn này." });

            if (interview.TrangThai != TrangThaiLichHen.ChoXacNhan)
                return Json(new { success = false, message = "Lịch phỏng vấn này không thể xác nhận ở trạng thái hiện tại." });

            interview.TrangThai = TrangThaiLichHen.DaXacNhan;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xác nhận chấp nhận lịch hẹn phỏng vấn!" });
        }

        /// <summary>
        /// Ứng viên từ chối lịch hẹn phỏng vấn.
        /// Chỉ cho phép từ chối khi lịch đang ở trạng thái ChoXacNhan.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeclineInterview(int maLichHen)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            var interview = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation)
                .FirstOrDefaultAsync(l => l.MaLichHen == maLichHen && l.MaDonNavigation.MaUngVien == userId);

            if (interview == null)
                return Json(new { success = false, message = "Không tìm thấy lịch phỏng vấn này." });

            if (interview.TrangThai != TrangThaiLichHen.ChoXacNhan)
                return Json(new { success = false, message = "Lịch phỏng vấn này không thể từ chối ở trạng thái hiện tại." });

            interview.TrangThai = TrangThaiLichHen.DaHuy;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xác nhận từ chối lịch hẹn phỏng vấn!" });
        }

        // =====================================================================
        // HỒ SƠ CÁ NHÂN
        // =====================================================================

        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var candidate = await _context.UngViens
                .Include(u => u.ChiTietKyNangUngViens).ThenInclude(ck => ck.MaKyNangNavigation)
                .FirstOrDefaultAsync(u => u.MaUngVien == userId);

            if (candidate == null) return RedirectToAction("Login", "Auth");

            return View(candidate);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại." });

            if (model == null || string.IsNullOrWhiteSpace(model.HoTen))
                return Json(new { success = false, message = "Họ tên không được để trống." });

            try
            {
                var candidate = await _context.UngViens.FirstOrDefaultAsync(u => u.MaUngVien == userId);
                if (candidate == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin ứng viên." });

                candidate.HoTen           = model.HoTen;
                candidate.SoDienThoai     = model.SoDienThoai;
                candidate.SoNamKinhNghiem = model.SoNamKinhNghiem ?? 0;
                candidate.LinkLinkedIn    = model.LinkLinkedIn;
                candidate.ChucDanhHienTai = model.ChucDanhHienTai;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật hồ sơ cá nhân thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================================
        // QUẢN LÝ KỸ NĂNG
        // =====================================================================

        /// <summary>
        /// Lấy danh sách kỹ năng chưa có trong hồ sơ của ứng viên để hiển thị lên dropdown thêm kỹ năng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAvailableSkills()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            try
            {
                // Lấy danh sách MaKyNang ứng viên đã có
                var existingSkillIds = await _context.ChiTietKyNangUngViens
                    .Where(c => c.MaUngVien == userId)
                    .Select(c => c.MaKyNang)
                    .ToListAsync();

                // Lọc ra những kỹ năng chưa có trong hồ sơ
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

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillRequest model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            try
            {
                if (await _context.ChiTietKyNangUngViens.AnyAsync(c => c.MaUngVien == userId && c.MaKyNang == model.MaKyNang))
                    return Json(new { success = false, message = "Kỹ năng này đã tồn tại trong hồ sơ của bạn." });

                _context.ChiTietKyNangUngViens.Add(new ChiTietKyNangUngVien
                {
                    MaUngVien       = userId,
                    MaKyNang        = model.MaKyNang,
                    SoNamKinhNghiem = 0
                });
                await _context.SaveChangesAsync();

                var skillName = await _context.DanhMucKyNangs
                    .Where(s => s.MaKyNang == model.MaKyNang)
                    .Select(s => s.TenKyNang)
                    .FirstOrDefaultAsync();

                return Json(new { success = true, message = "Thêm kỹ năng thành công!", skillId = model.MaKyNang, skillName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSkill([FromBody] DeleteSkillRequest model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            try
            {
                var skillDetail = await _context.ChiTietKyNangUngViens
                    .FirstOrDefaultAsync(c => c.MaUngVien == userId && c.MaKyNang == model.MaKyNang);

                if (skillDetail == null)
                    return Json(new { success = false, message = "Không tìm thấy kỹ năng này." });

                _context.ChiTietKyNangUngViens.Remove(skillDetail);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa kỹ năng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
