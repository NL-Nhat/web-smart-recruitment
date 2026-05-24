using System;

namespace web_smart_recruitment.Models.ViewModels
{
    public class AdminUserViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string Email { get; set; } = null!;
        public string TenHienThi { get; set; } = null!;
        public string TenVaiTro { get; set; } = null!;
        public bool TrangThaiHoatDong { get; set; }
    }
}
