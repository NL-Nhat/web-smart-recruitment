using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using web_smart_recruitment.Models;

namespace web_smart_recruitment.Controllers
{
    [Authorize(Roles = "UngVien")]
    public class CandidateController : Controller
    {
        private readonly AppDbContext _context;

        public CandidateController(AppDbContext context)
        {
            _context = context;
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
