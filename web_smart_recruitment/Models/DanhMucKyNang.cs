using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class DanhMucKyNang
{
    public int MaKyNang { get; set; }

    public string TenKyNang { get; set; } = null!;

    public string? PhanLoai { get; set; }

    public virtual ICollection<ChiTietKyNangTinTuyenDung> ChiTietKyNangTinTuyenDungs { get; set; } = new List<ChiTietKyNangTinTuyenDung>();

    public virtual ICollection<ChiTietKyNangUngVien> ChiTietKyNangUngViens { get; set; } = new List<ChiTietKyNangUngVien>();
}
