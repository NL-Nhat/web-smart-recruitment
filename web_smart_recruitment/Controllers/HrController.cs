using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.Dtos;
using web_smart_recruitment.Enums;

namespace web_smart_recruitment.Controllers
{
    /// <summary>
    /// Controller cho Nhà tuyển dụng (HR):
    /// Quản lý tin tuyển dụng, ứng viên, lịch phỏng vấn, hồ sơ công ty.
    /// </summary>
    [Authorize(Roles = "NhaTuyenDung")]
    public class HrController : Controller
    {
        private readonly AppDbContext _context;

        public HrController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        /// <summary>
        /// Lấy ID tài khoản của HR đang đăng nhập từ JWT Claims.
        /// Trả về 0 nếu chưa đăng nhập hoặc Claims không hợp lệ.
        /// </summary>
        private int GetCurrentUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        /// <summary>
        /// Chuyển đổi DateTime sang chuỗi thời gian tương đối (ví dụ: "5 phút trước").
        /// Dùng để hiển thị trên Activity feed trong Dashboard.
        /// </summary>
        public static string GetRelativeTime(DateTime dt)
        {
            var ts = DateTime.Now - dt;
            if (ts.TotalSeconds < 0 || ts.TotalMinutes < 1) return "vừa xong";
            if (ts.TotalMinutes < 60)  return $"{(int)ts.TotalMinutes} phút trước";
            if (ts.TotalHours < 24)    return $"{(int)ts.TotalHours} giờ trước";
            if (ts.TotalDays < 30)     return $"{(int)ts.TotalDays} ngày trước";
            return dt.ToString("dd/MM/yyyy HH:mm");
        }

        // =====================================================================
        // DASHBOARD
        // =====================================================================

        public async Task<IActionResult> Dashboard()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Tên công ty hiển thị trên Navbar
            var employer = await _context.NhaTuyenDungs.FirstOrDefaultAsync(n => n.MaNhaTuyenDung == userId);
            ViewBag.CompanyName = employer?.TenCongTy ?? User.Identity?.Name ?? "Nhà tuyển dụng";

            // 1. Tin đang mở (chưa bị xóa và đang ở trạng thái DangMo)
            ViewBag.ActiveJobsCount = await _context.TinTuyenDungs
                .CountAsync(t => t.MaNhaTuyenDung == userId
                              && t.TrangThai == TrangThaiTin.DangMo
                              && (t.DaXoa == false || t.DaXoa == null));

            // 2. Tổng ứng viên đã nộp đơn vào các tin của HR này
            ViewBag.TotalCandidatesCount = await _context.DonUngTuyens
                .CountAsync(d => d.MaTinNavigation.MaNhaTuyenDung == userId
                              && (d.MaTinNavigation.DaXoa == false || d.MaTinNavigation.DaXoa == null));

            // 3. Số người được tuyển trong tháng hiện tại
            int currentMonth = DateTime.Today.Month;
            int currentYear  = DateTime.Today.Year;
            ViewBag.HiredThisMonthCount = await _context.DonUngTuyens
                .CountAsync(d => d.MaTinNavigation.MaNhaTuyenDung == userId
                              && d.TrangThai == TrangThaiDon.TrungTuyen
                              && ((d.NgayCapNhat != null && d.NgayCapNhat.Value.Month == currentMonth && d.NgayCapNhat.Value.Year == currentYear)
                               || (d.NgayCapNhat == null && d.NgayNop != null && d.NgayNop.Value.Month == currentMonth && d.NgayNop.Value.Year == currentYear)));

            // 4. Điểm AI trung bình (chỉ tính những kết quả đã HoanThanh)
            ViewBag.AvgAiScore = await _context.KetQuaAis
                .Where(k => k.MaDonNavigation.MaTinNavigation.MaNhaTuyenDung == userId
                         && k.TrangThaiXuLy == TrangThaiKetQuaAi.HoanThanh
                         && k.DiemPhuHop != null)
                .AverageAsync(k => (double?)k.DiemPhuHop) ?? 0.0;

            // 5. Tin tuyển dụng gần đây (Top 5)
            ViewBag.RecentJobs = await _context.TinTuyenDungs
                .Include(t => t.DonUngTuyens)
                .Where(t => t.MaNhaTuyenDung == userId && (t.DaXoa == false || t.DaXoa == null))
                .OrderByDescending(t => t.NgayTao)
                .Take(5)
                .ToListAsync();

            // 6. Hoạt động gần đây — kết hợp 3 loại sự kiện thành 1 timeline
            ViewBag.RecentActivities = await BuildDashboardActivitiesAsync(userId);

            return View();
        }

        /// <summary>
        /// Xây dựng timeline hoạt động cho HR Dashboard bằng cách gộp 3 loại sự kiện:
        /// Nộp đơn, AI phân tích xong, Lịch hẹn phỏng vấn.
        /// SVG icon được inline để không cần file ảnh riêng.
        /// </summary>
        private async Task<List<HrActivityDto>> BuildDashboardActivitiesAsync(int userId)
        {
            var activities = new List<HrActivityDto>();

            // Sự kiện ứng viên nộp đơn
            var recentApps = await _context.DonUngTuyens
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.MaTinNavigation)
                .Where(d => d.MaTinNavigation.MaNhaTuyenDung == userId)
                .OrderByDescending(d => d.NgayNop)
                .Take(5)
                .ToListAsync();

            foreach (var app in recentApps.Where(a => a.NgayNop.HasValue))
            {
                activities.Add(new HrActivityDto
                {
                    Time        = app.NgayNop!.Value,
                    Title       = app.MaUngVienNavigation?.HoTen ?? "Ứng viên",
                    Description = $"vừa nộp đơn vào {app.MaTinNavigation?.TieuDe ?? "tin tuyển dụng"}",
                    IconHtml    = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2\"></path><circle cx=\"12\" cy=\"7\" r=\"4\"></circle></svg>",
                    IconColor   = ""
                });
            }

            // Sự kiện AI phân tích xong
            var recentAiResults = await _context.KetQuaAis
                .Include(k => k.MaDonNavigation).ThenInclude(d => d.MaUngVienNavigation)
                .Include(k => k.MaDonNavigation).ThenInclude(d => d.MaTinNavigation)
                .Where(k => k.MaDonNavigation.MaTinNavigation.MaNhaTuyenDung == userId
                         && k.TrangThaiXuLy == TrangThaiKetQuaAi.HoanThanh)
                .OrderByDescending(k => k.NgayPhanTich)
                .Take(5)
                .ToListAsync();

            foreach (var ai in recentAiResults.Where(k => k.NgayPhanTich.HasValue))
            {
                activities.Add(new HrActivityDto
                {
                    Time        = ai.NgayPhanTich!.Value,
                    Title       = "AI Analysis Hoàn tất",
                    Description = $"Vị trí {ai.MaDonNavigation?.MaTinNavigation?.TieuDe ?? "tin tuyển dụng"} có ứng viên {ai.MaDonNavigation?.MaUngVienNavigation?.HoTen ?? "mới"} đạt {ai.DiemPhuHop:F0}%",
                    IconHtml    = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z\"></path></svg>",
                    IconColor   = "color: var(--el-success);"
                });
            }

            // Sự kiện lịch hẹn phỏng vấn mới
            var recentInterviews = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaUngVienNavigation)
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaTinNavigation)
                .Where(l => l.MaDonNavigation.MaTinNavigation.MaNhaTuyenDung == userId)
                .OrderByDescending(l => l.NgayTao)
                .Take(5)
                .ToListAsync();

            foreach (var iv in recentInterviews.Where(l => l.NgayTao.HasValue))
            {
                activities.Add(new HrActivityDto
                {
                    Time        = iv.NgayTao!.Value,
                    Title       = "Lên lịch phỏng vấn",
                    Description = $"Đã lên lịch với {iv.MaDonNavigation?.MaUngVienNavigation?.HoTen ?? "ứng viên"} cho vị trí {iv.MaDonNavigation?.MaTinNavigation?.TieuDe ?? "tin tuyển dụng"}",
                    IconHtml    = "<svg width=\"14\" height=\"14\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"18\" rx=\"2\" ry=\"2\"></rect><line x1=\"16\" y1=\"2\" x2=\"16\" y2=\"6\"></line><line x1=\"8\" y1=\"2\" x2=\"8\" y2=\"6\"></line><line x1=\"3\" y1=\"10\" x2=\"21\" y2=\"10\"></line></svg>",
                    IconColor   = "color: var(--el-warning);"
                });
            }

            // Gộp và lấy 5 hoạt động mới nhất
            return activities.OrderByDescending(a => a.Time).Take(5).ToList();
        }

        // =====================================================================
        // QUẢN LÝ TIN TUYỂN DỤNG
        // =====================================================================

        /// <summary>
        /// Danh sách tin tuyển dụng của HR, có lọc theo trạng thái và phân trang.
        /// Chỉ hiển thị tin chưa bị xóa (DaXoa = false hoặc null).
        /// </summary>
        public async Task<IActionResult> Jobs(string status = null, int page = 1)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Lọc tin của HR này, chưa bị xóa
            var query = _context.TinTuyenDungs
                .Where(t => t.MaNhaTuyenDung == userId && (t.DaXoa == false || t.DaXoa == null));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.TrangThai == status);

            int totalRecords = await query.CountAsync();
            int pageSize     = 10;
            int totalPages   = (int)Math.Ceiling((double)totalRecords / pageSize);

            var jobs = await query
                .OrderByDescending(t => t.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage   = page;
            ViewBag.TotalPages    = totalPages;
            ViewBag.CurrentStatus = status;

            return View(jobs);
        }

        /// <summary>
        /// Form tạo mới hoặc chỉnh sửa tin tuyển dụng.
        /// Nếu có maTin thì load dữ liệu để chỉnh sửa, nếu không thì tạo mới.
        /// </summary>
        public async Task<IActionResult> JobForm(int? maTin)
        {
            ViewBag.Skills = await _context.DanhMucKyNangs.ToListAsync();

            if (maTin.HasValue)
            {
                var job = await _context.TinTuyenDungs
                    .Include(t => t.ChiTietKyNangTinTuyenDungs)
                    .FirstOrDefaultAsync(t => t.MaTin == maTin.Value);

                if (job == null) return NotFound();
                return View(job);
            }

            return View();
        }

        /// <summary>
        /// Tạo tin tuyển dụng mới. Mặc định trạng thái là DangMo.
        /// Lưu kèm danh sách kỹ năng yêu cầu.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateJob(TinTuyenDung model, List<int> skillIds, List<string> skillLevels)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            ViewBag.Skills = await _context.DanhMucKyNangs.ToListAsync();

            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(model.TieuDe) || string.IsNullOrWhiteSpace(model.MoTaCongViec) || string.IsNullOrWhiteSpace(model.YeuCauCongViec))
            {
                ModelState.AddModelError("", "Tiêu đề, Mô tả và Yêu cầu công việc không được để trống.");
                return View("JobForm", model);
            }
            if (model.HanNopCv.HasValue && model.HanNopCv.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("HanNopCv", "Hạn nộp CV không được nhỏ hơn ngày hiện tại.");
                return View("JobForm", model);
            }
            if (model.MucLuongToiThieu.HasValue && model.MucLuongToiThieu.Value < 0)
            {
                ModelState.AddModelError("MucLuongToiThieu", "Mức lương tối thiểu không được âm.");
                return View("JobForm", model);
            }
            if (model.MucLuongToiThieu.HasValue && model.MucLuongToiDa.HasValue && model.MucLuongToiDa.Value < model.MucLuongToiThieu.Value)
            {
                ModelState.AddModelError("MucLuongToiDa", "Mức lương tối đa phải lớn hơn hoặc bằng mức lương tối thiểu.");
                return View("JobForm", model);
            }

            model.MaNhaTuyenDung = userId;
            model.TrangThai      = TrangThaiTin.DangMo;
            model.NgayTao        = DateTime.Now;
            model.NgayCapNhat    = DateTime.Now;
            model.DaXoa          = false;

            _context.TinTuyenDungs.Add(model);
            await _context.SaveChangesAsync();

            // Lưu danh sách kỹ năng yêu cầu (nếu có)
            if (skillIds?.Count > 0 && skillLevels?.Count == skillIds.Count)
            {
                var kyNangList = skillIds.Select((id, i) => new ChiTietKyNangTinTuyenDung
                {
                    MaTin       = model.MaTin,
                    MaKyNang    = id,
                    CapDoYeuCau = skillLevels[i]
                });
                _context.ChiTietKyNangTinTuyenDungs.AddRange(kyNangList);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Jobs");
        }

        /// <summary>
        /// Cập nhật tin tuyển dụng đã có.
        /// Chỉ HR sở hữu tin mới được phép cập nhật (bảo mật theo userId).
        /// Xóa kỹ năng cũ và lưu kỹ năng mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EditJob(int maTin, TinTuyenDung model, List<int> skillIds, List<string> skillLevels)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            ViewBag.Skills = await _context.DanhMucKyNangs.ToListAsync();

            // Validation (giống CreateJob)
            if (string.IsNullOrWhiteSpace(model.TieuDe) || string.IsNullOrWhiteSpace(model.MoTaCongViec) || string.IsNullOrWhiteSpace(model.YeuCauCongViec))
            {
                ModelState.AddModelError("", "Tiêu đề, Mô tả và Yêu cầu công việc không được để trống.");
                return View("JobForm", model);
            }
            if (model.HanNopCv.HasValue && model.HanNopCv.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("HanNopCv", "Hạn nộp CV không được nhỏ hơn ngày hiện tại.");
                return View("JobForm", model);
            }
            if (model.MucLuongToiThieu.HasValue && model.MucLuongToiThieu.Value < 0)
            {
                ModelState.AddModelError("MucLuongToiThieu", "Mức lương tối thiểu không được âm.");
                return View("JobForm", model);
            }
            if (model.MucLuongToiThieu.HasValue && model.MucLuongToiDa.HasValue && model.MucLuongToiDa.Value < model.MucLuongToiThieu.Value)
            {
                ModelState.AddModelError("MucLuongToiDa", "Mức lương tối đa phải lớn hơn hoặc bằng mức lương tối thiểu.");
                return View("JobForm", model);
            }

            // Kiểm tra quyền sở hữu tin — chỉ HR này mới được sửa
            var job = await _context.TinTuyenDungs
                .Include(t => t.ChiTietKyNangTinTuyenDungs)
                .FirstOrDefaultAsync(t => t.MaTin == maTin && t.MaNhaTuyenDung == userId);

            if (job == null) return NotFound();

            job.TieuDe          = model.TieuDe;
            job.PhongBan        = model.PhongBan;
            job.DiaDiem         = model.DiaDiem;
            job.HinhThucLamViec = model.HinhThucLamViec;
            job.HanNopCv        = model.HanNopCv;
            job.MucLuongToiThieu = model.MucLuongToiThieu;
            job.MucLuongToiDa   = model.MucLuongToiDa;
            job.MoTaCongViec    = model.MoTaCongViec;
            job.YeuCauCongViec  = model.YeuCauCongViec;
            job.QuyenLoi        = model.QuyenLoi;
            job.TrangThai       = model.TrangThai;
            job.NgayCapNhat     = DateTime.Now;

            // Xóa kỹ năng cũ và thêm kỹ năng mới
            _context.ChiTietKyNangTinTuyenDungs.RemoveRange(job.ChiTietKyNangTinTuyenDungs);

            if (skillIds?.Count > 0 && skillLevels?.Count == skillIds.Count)
            {
                var kyNangList = skillIds.Select((id, i) => new ChiTietKyNangTinTuyenDung
                {
                    MaTin       = job.MaTin,
                    MaKyNang    = id,
                    CapDoYeuCau = skillLevels[i]
                });
                _context.ChiTietKyNangTinTuyenDungs.AddRange(kyNangList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Jobs");
        }

        /// <summary>
        /// Xóa mềm (Soft Delete) tin tuyển dụng.
        /// Không xóa khỏi DB mà chỉ đánh dấu DaXoa = true.
        /// Chỉ HR sở hữu tin mới được phép xóa (bảo mật theo userId).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteJob(int maTin)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var job = await _context.TinTuyenDungs
                .FirstOrDefaultAsync(t => t.MaTin == maTin && t.MaNhaTuyenDung == userId);

            if (job != null)
            {
                job.DaXoa       = true;
                job.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Jobs");
        }

        // =====================================================================
        // QUẢN LÝ ỨNG VIÊN
        // =====================================================================

        /// <summary>
        /// Danh sách đơn ứng tuyển (ATS Dashboard).
        /// Sắp xếp ứng viên theo điểm AI giảm dần (phù hợp nhất lên đầu).
        /// </summary>
        public async Task<IActionResult> Applications(int? maTin)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var query = _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.KetQuaAis) // Load điểm AI để sắp xếp
                .Where(d => d.MaTinNavigation.MaNhaTuyenDung == userId);

            // Lọc theo tin cụ thể nếu có
            if (maTin.HasValue)
            {
                query = query.Where(d => d.MaTin == maTin.Value);
                var job = await _context.TinTuyenDungs.FirstOrDefaultAsync(t => t.MaTin == maTin.Value);
                if (job != null) ViewBag.JobTitle = job.TieuDe;
            }

            var applications = await query.ToListAsync();

            // Sắp xếp theo điểm AI mới nhất giảm dần — giúp HR thấy ứng viên phù hợp nhất trước
            var sorted = applications
                .OrderByDescending(d => d.KetQuaAis.OrderByDescending(k => k.NgayPhanTich).FirstOrDefault()?.DiemPhuHop ?? 0)
                .ToList();

            return View(sorted);
        }

        // =====================================================================
        // QUẢN LÝ LỊCH HẸN PHỎNG VẤN
        // =====================================================================

        /// <summary>
        /// Danh sách lịch hẹn phỏng vấn, hỗ trợ lọc theo tin/trạng thái và phân trang.
        /// </summary>
        public async Task<IActionResult> Interviews(int page = 1, int? jobId = null, string status = null, string viewMode = "list")
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var query = _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaUngVienNavigation)
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaTinNavigation)
                .Where(l => l.MaDonNavigation.MaTinNavigation.MaNhaTuyenDung == userId);

            if (jobId.HasValue)
                query = query.Where(l => l.MaDonNavigation.MaTin == jobId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.TrangThai == status);

            int pageSize     = 10;
            int totalRecords = await query.CountAsync();
            int totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var interviews = await query
                .OrderByDescending(l => l.NgayPhuongVan)
                .ThenByDescending(l => l.GioPhuongVan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Dữ liệu cho bộ lọc
            ViewBag.Jobs = await _context.TinTuyenDungs
                .Where(t => t.MaNhaTuyenDung == userId && (t.DaXoa == false || t.DaXoa == null))
                .ToListAsync();

            ViewBag.Statuses = await _context.LichHenPhongVans
                .Where(l => !string.IsNullOrEmpty(l.TrangThai))
                .Select(l => l.TrangThai)
                .Distinct()
                .ToListAsync();

            // Danh sách hình thức cho form cập nhật
            ViewBag.HinhThucList = new List<string> { HinhThucPhongVan.Online, HinhThucPhongVan.Offline };

            ViewBag.CurrentPage   = page;
            ViewBag.TotalPages    = totalPages;
            ViewBag.SelectedJobId = jobId;
            ViewBag.SelectedStatus = status;
            ViewBag.ViewMode      = viewMode;

            return View(interviews);
        }

        // =====================================================================
        // XEM CHI TIẾT ĐÁNH GIÁ AI
        // =====================================================================

        /// <summary>
        /// Xem chi tiết kết quả phân tích AI cho một đơn ứng tuyển.
        /// Load kèm danh sách lịch hẹn phỏng vấn để kiểm tra điều kiện hiển thị nút hành động.
        /// </summary>
        public async Task<IActionResult> AiCandidate(int maDon)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            // Load đầy đủ dữ liệu liên quan: thông tin ứng viên, tin tuyển dụng, kết quả AI, lịch hẹn
            var application = await _context.DonUngTuyens
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.MaTinNavigation)
                .Include(d => d.KetQuaAis)
                .Include(d => d.LichHenPhongVans) // Cần để kiểm tra lịch hẹn có bị hủy không
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTinNavigation.MaNhaTuyenDung == userId);

            if (application == null) return NotFound();

            // Danh sách hình thức phỏng vấn cho modal Đặt lịch
            ViewBag.HinhThucList = new List<string> { HinhThucPhongVan.Online, HinhThucPhongVan.Offline };

            return View(application);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn ứng tuyển (Chấp nhận, Từ chối, Trúng tuyển...).
        /// Chỉ HR sở hữu đơn mới được cập nhật (bảo mật theo userId).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(int maDon, string newStatus, string? returnUrl = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var application = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == maDon
                                       && d.MaTinNavigation != null
                                       && d.MaTinNavigation.MaNhaTuyenDung == userId);

            if (application != null)
            {
                application.TrangThai = newStatus;
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("AiCandidate", new { maDon });
        }

        /// <summary>
        /// Đặt lịch hẹn phỏng vấn cho một đơn ứng tuyển.
        ///
        /// Lưu ý DB constraint (CHK_LichHen_DiaDiem_LinkHop):
        /// - Online → LinkHop phải có, DiaDiem phải NULL
        /// - Offline → DiaDiem phải có, LinkHop phải NULL
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ScheduleInterview(
            int maDon, DateOnly NgayPhuongVan, TimeOnly GioPhuongVan,
            string HinhThuc, string LinkHop, string DiaDiem, string GhiChu)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var application = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTinNavigation.MaNhaTuyenDung == userId);

            if (application != null)
            {
                bool isOnline = HinhThuc == HinhThucPhongVan.Online;

                var lichHen = new LichHenPhongVan
                {
                    MaDon         = maDon,
                    NgayPhuongVan = NgayPhuongVan,
                    GioPhuongVan  = GioPhuongVan,
                    HinhThuc      = HinhThuc,
                    // Chỉ gán đúng trường theo hình thức — tránh vi phạm CHK constraint của DB
                    LinkHop       = isOnline  ? (string.IsNullOrWhiteSpace(LinkHop) ? null : LinkHop)   : null,
                    DiaDiem       = !isOnline ? (string.IsNullOrWhiteSpace(DiaDiem) ? null : DiaDiem)   : null,
                    GhiChu        = string.IsNullOrWhiteSpace(GhiChu) ? null : GhiChu,
                    TrangThai     = TrangThaiLichHen.ChoXacNhan,
                    NgayTao       = DateTime.Now
                };

                _context.LichHenPhongVans.Add(lichHen);
                application.TrangThai = TrangThaiDon.PhongVan; // Cập nhật trạng thái đơn
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AiCandidate", new { maDon });
        }

        /// <summary>
        /// Cập nhật thông tin lịch hẹn phỏng vấn.
        /// Chỉ cho phép cập nhật khi trạng thái là ChoXacNhan hoặc DaXacNhan.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateInterview(
            int maDon, DateOnly NgayPhuongVan, TimeOnly GioPhuongVan,
            string HinhThuc, string LinkHop, string DiaDiem, string GhiChu, string? TrangThai)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var lichHen = await _context.LichHenPhongVans
                .Include(l => l.MaDonNavigation).ThenInclude(d => d.MaTinNavigation)
                .FirstOrDefaultAsync(l => l.MaDon == maDon && l.MaDonNavigation.MaTinNavigation.MaNhaTuyenDung == userId);

            // Chỉ cho phép sửa khi lịch chưa kết thúc hoặc chưa bị hủy
            if (lichHen != null
                && (lichHen.TrangThai == TrangThaiLichHen.ChoXacNhan
                 || lichHen.TrangThai == TrangThaiLichHen.DaXacNhan))
            {
                bool isOnline = HinhThuc == HinhThucPhongVan.Online;

                lichHen.NgayPhuongVan = NgayPhuongVan;
                lichHen.GioPhuongVan  = GioPhuongVan;
                lichHen.HinhThuc      = HinhThuc;
                lichHen.LinkHop       = isOnline  ? (string.IsNullOrWhiteSpace(LinkHop) ? null : LinkHop)   : null;
                lichHen.DiaDiem       = !isOnline ? (string.IsNullOrWhiteSpace(DiaDiem) ? null : DiaDiem)   : null;
                lichHen.GhiChu        = string.IsNullOrWhiteSpace(GhiChu) ? null : GhiChu;

                // Chuyển trạng thái theo quy trình: ChoXacNhan → DaHuy, DaXacNhan → HoanThanh
                if (!string.IsNullOrEmpty(TrangThai))
                {
                    if (lichHen.TrangThai == TrangThaiLichHen.ChoXacNhan && TrangThai == TrangThaiLichHen.DaHuy)
                        lichHen.TrangThai = TrangThaiLichHen.DaHuy;
                    else if (lichHen.TrangThai == TrangThaiLichHen.DaXacNhan && TrangThai == TrangThaiLichHen.HoanThanh)
                        lichHen.TrangThai = TrangThaiLichHen.HoanThanh;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Interviews");
        }

        // =====================================================================
        // HỒ SƠ CÔNG TY
        // =====================================================================

        public IActionResult JobStatus() => View();
        public IActionResult Company() => View();

        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var employer = await _context.NhaTuyenDungs.FirstOrDefaultAsync(n => n.MaNhaTuyenDung == userId);
            if (employer == null) return NotFound();

            return View(employer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(NhaTuyenDung model, IFormFile? logoFile, IFormFile? anhBiaFile)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Auth");

            var existingEmployer = await _context.NhaTuyenDungs.FirstOrDefaultAsync(n => n.MaNhaTuyenDung == userId);
            if (existingEmployer == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.TenCongTy))
                ModelState.AddModelError("TenCongTy", "Tên công ty không được để trống.");

            // Bỏ qua validation của Navigation property và file upload (không bind từ form)
            ModelState.Remove("MaNhaTuyenDungNavigation");
            ModelState.Remove("logoFile");
            ModelState.Remove("anhBiaFile");

            if (!ModelState.IsValid)
            {
                existingEmployer.TenCongTy  = model.TenCongTy;
                existingEmployer.Website    = model.Website;
                existingEmployer.SoDienThoai = model.SoDienThoai;
                existingEmployer.DiaChi     = model.DiaChi;
                existingEmployer.MoTa       = model.MoTa;
                return View("Profile", existingEmployer);
            }

            // Xử lý upload ảnh và cập nhật profile
            if (logoFile?.Length > 0)
                existingEmployer.Logo = await SaveImageAsync(logoFile, existingEmployer.Logo);

            if (anhBiaFile?.Length > 0)
                existingEmployer.AnhBia = await SaveImageAsync(anhBiaFile, existingEmployer.AnhBia);

            existingEmployer.TenCongTy  = model.TenCongTy;
            existingEmployer.Website    = model.Website;
            existingEmployer.SoDienThoai = model.SoDienThoai;
            existingEmployer.DiaChi     = model.DiaChi;
            existingEmployer.MoTa       = model.MoTa;

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// Helper: Lưu file ảnh vào thư mục wwwroot/img và xóa ảnh cũ (nếu có).
        /// Trả về đường dẫn tương đối mới (ví dụ: "/img/abc123.jpg").
        /// </summary>
        private static async Task<string> SaveImageAsync(IFormFile imageFile, string? oldImagePath)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            // Xóa file cũ để tránh lãng phí dung lượng
            if (!string.IsNullOrEmpty(oldImagePath))
            {
                var oldFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFullPath)) System.IO.File.Delete(oldFullPath);
            }

            return "/img/" + fileName;
        }
    }
}
