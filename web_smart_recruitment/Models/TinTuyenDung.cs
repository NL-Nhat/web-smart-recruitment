using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class TinTuyenDung
{
    public int MaTin { get; set; }

    public int? MaNhaTuyenDung { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? PhongBan { get; set; }

    public string? DiaDiem { get; set; }

    public string? HinhThucLamViec { get; set; }

    public decimal? MucLuongToiThieu { get; set; }

    public decimal? MucLuongToiDa { get; set; }

    public string MoTaCongViec { get; set; } = null!;

    public string YeuCauCongViec { get; set; } = null!;

    public string? QuyenLoi { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? HanNopCv { get; set; }

    public bool? DaXoa { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiTietKyNangTinTuyenDung> ChiTietKyNangTinTuyenDungs { get; set; } = new List<ChiTietKyNangTinTuyenDung>();

    public virtual ICollection<DonUngTuyen> DonUngTuyens { get; set; } = new List<DonUngTuyen>();

    public virtual NhaTuyenDung? MaNhaTuyenDungNavigation { get; set; }
}
