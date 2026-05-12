using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class NhaTuyenDung
{
    public int MaNhaTuyenDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string? TenCongTy { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Website { get; set; }

    public string? DiaChi { get; set; }

    public string? MoTa { get; set; }

    public string? Logo { get; set; }

    public string? AnhBia { get; set; }

    public virtual TaiKhoan MaNhaTuyenDungNavigation { get; set; } = null!;

    public virtual ICollection<TinTuyenDung> TinTuyenDungs { get; set; } = new List<TinTuyenDung>();
}
