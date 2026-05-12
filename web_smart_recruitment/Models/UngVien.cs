using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class UngVien
{
    public int MaUngVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? LinkLinkedIn { get; set; }

    public string? ChucDanhHienTai { get; set; }

    public int? SoNamKinhNghiem { get; set; }

    public string? AnhDaiDien { get; set; }

    public string? GioiThieu { get; set; }

    public virtual ICollection<ChiTietKyNangUngVien> ChiTietKyNangUngViens { get; set; } = new List<ChiTietKyNangUngVien>();

    public virtual ICollection<DonUngTuyen> DonUngTuyens { get; set; } = new List<DonUngTuyen>();

    public virtual ICollection<HoSoCv> HoSoCvs { get; set; } = new List<HoSoCv>();

    public virtual TaiKhoan MaUngVienNavigation { get; set; } = null!;
}
