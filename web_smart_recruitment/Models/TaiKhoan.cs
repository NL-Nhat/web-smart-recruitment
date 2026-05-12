using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string Email { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public int MaVaiTro { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    public virtual NhaTuyenDung? NhaTuyenDung { get; set; }

    public virtual UngVien? UngVien { get; set; }
}
