using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_smart_recruitment.Models;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "NhaTuyenDung")]
    public class HrController : Controller
    {
        private readonly AppDbContext _context;

        public HrController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard() => View();
        
        // Chức năng Xem danh sách tin tuyển dụng của nhà tuyển dụng
        public async Task<IActionResult> Jobs()
        {
            // 1. Lấy ID tài khoản của Nhà tuyển dụng đang đăng nhập từ Claims
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Do quan hệ 1-1, MaNhaTuyenDung chính là MaTaiKhoan (userId)
            int maNhaTuyenDung = userId;

            // 2. Dùng LINQ để lấy danh sách tin tuyển dụng thuộc về nhà tuyển dụng này
            // - Lọc theo MaNhaTuyenDung
            // - Lọc các tin chưa bị xóa (DaXoa == false hoặc null)
            // - Sắp xếp theo ngày tạo mới nhất
            var jobs = await _context.TinTuyenDungs
                .Where(t => t.MaNhaTuyenDung == maNhaTuyenDung && (t.DaXoa == false || t.DaXoa == null))
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            // 3. Trả dữ liệu về cho View hiển thị
            return View(jobs);
        }

        // Chức năng Xem danh sách ứng viên (ATS Dashboard)
        public async Task<IActionResult> Applications(int? maTin)
        {
            // 1. Lấy ID tài khoản của Nhà tuyển dụng đang đăng nhập từ Claims
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            int maNhaTuyenDung = userId;

            // 2. Dùng LINQ để lấy danh sách Đơn ứng tuyển
            // Chúng ta cần join với bảng TinTuyenDung để đảm bảo tin đó thuộc về nhà tuyển dụng này
            var query = _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.KetQuaAis) // Kéo theo Kết quả AI để hiển thị điểm
                .Where(d => d.MaTinNavigation.MaNhaTuyenDung == maNhaTuyenDung);

            // 3. Nếu có tham số maTin (từ trang Danh sách tin truyền qua), lọc thêm theo maTin đó
            if (maTin.HasValue)
            {
                query = query.Where(d => d.MaTin == maTin.Value);
                
                // Lấy tên tin để hiển thị trên View
                var job = await _context.TinTuyenDungs.FirstOrDefaultAsync(t => t.MaTin == maTin.Value);
                if (job != null)
                {
                    ViewBag.JobTitle = job.TieuDe;
                }
            }

            // 4. Lấy dữ liệu và tính điểm AI lớn nhất cho mỗi đơn (nếu có nhiều lần phân tích)
            // Sắp xếp giảm dần theo điểm AI để hiển thị ứng viên phù hợp nhất lên đầu
            var applications = await query.ToListAsync();
            var sortedApplications = applications.OrderByDescending(d => 
                d.KetQuaAis.OrderByDescending(k => k.NgayPhanTich).FirstOrDefault()?.DiemPhuHop ?? 0)
                .ToList();

            return View(sortedApplications);
        }
        public IActionResult Interviews() => View();
        public IActionResult JobForm() => View();
        public IActionResult JobStatus() => View();
        public IActionResult Company() => View();
        public IActionResult Profile() => View();
        // Chức năng Xem chi tiết đánh giá AI
        public async Task<IActionResult> AiCandidate(int maDon)
        {
            // 1. Kiểm tra đăng nhập và lấy ID Nhà tuyển dụng
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }
            int maNhaTuyenDung = userId;

            // 2. Lấy thông tin đơn ứng tuyển kèm theo các bảng liên quan
            // Sử dụng LINQ với Include để nạp dữ liệu từ các bảng: UngVien, TinTuyenDung, HoSoCv và KetQuaAi
            var application = await _context.DonUngTuyens
                .Include(d => d.MaUngVienNavigation)
                .Include(d => d.MaTinNavigation)
                .Include(d => d.MaCvNavigation)
                .Include(d => d.KetQuaAis)
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTinNavigation.MaNhaTuyenDung == maNhaTuyenDung);

            // 3. Nếu không tìm thấy đơn (hoặc đơn không thuộc về nhà tuyển dụng này), trả về lỗi 404
            if (application == null)
            {
                return NotFound();
            }

            // 4. Truyền toàn bộ dữ liệu (Model) sang View để hiển thị
            ViewBag.HinhThucList = await _context.LichHenPhongVans
                .Where(l => !string.IsNullOrEmpty(l.HinhThuc))
                .Select(l => l.HinhThuc)
                .Distinct()
                .ToListAsync();

            return View(application);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(int maDon, string newStatus)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var application = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTinNavigation.MaNhaTuyenDung == userId);

            if (application != null)
            {
                application.TrangThai = newStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AiCandidate", new { maDon = maDon });
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleInterview(int maDon, DateOnly NgayPhuongVan, TimeOnly GioPhuongVan, string HinhThuc, string LinkHop, string DiaDiem, string GhiChu)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kiểm tra quyền sở hữu đơn ứng tuyển
            var application = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)
                .FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTinNavigation.MaNhaTuyenDung == userId);

            if (application != null)
            {
                // Tạo lịch hẹn mới
                var lichHen = new LichHenPhongVan
                {
                    MaDon = maDon,
                    NgayPhuongVan = NgayPhuongVan,
                    GioPhuongVan = GioPhuongVan,
                    HinhThuc = HinhThuc,
                    LinkHop = string.IsNullOrWhiteSpace(LinkHop) ? null : LinkHop,
                    DiaDiem = string.IsNullOrWhiteSpace(DiaDiem) ? null : DiaDiem,
                    GhiChu = string.IsNullOrWhiteSpace(GhiChu) ? null : GhiChu,
                    TrangThai = "ChoXacNhan",
                    NgayTao = DateTime.Now
                };

                _context.LichHenPhongVans.Add(lichHen);

                // Cập nhật trạng thái đơn ứng tuyển thành PhongVan
                application.TrangThai = "PhongVan";
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AiCandidate", new { maDon = maDon });
        }
    }
}
