using System;

namespace web_smart_recruitment.Models.ViewModels
{
    public class ApplicationsViewModel
    {
        public int MaDon { get; set; }
        public int MaTin { get; set; }
        public string TieuDeCongViec { get; set; } = null!;
        public string TenCongTy { get; set; } = null!;
        public string? LogoCongTy { get; set; }
        public DateTime NgayNop { get; set; }
        public string TrangThai { get; set; } = null!;
        public decimal? DiemPhuHop { get; set; }
        public string? TrangThaiAI { get; set; } // DangXuLy, HoanThanh, Loi
    }
}
