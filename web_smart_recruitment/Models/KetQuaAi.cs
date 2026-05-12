using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class KetQuaAi
{
    public int MaKetQua { get; set; }

    public int? MaDon { get; set; }

    public string? TrangThaiXuLy { get; set; }

    public decimal? DiemPhuHop { get; set; }

    public string? TomTatUngVien { get; set; }

    public string? KyNangPhuHopJson { get; set; }

    public string? KyNangThieuJson { get; set; }

    public string? DiemManh { get; set; }

    public string? DiemYeu { get; set; }

    public string? DeXuat { get; set; }

    public string? PhanHoiGocTuAi { get; set; }

    public DateTime? NgayPhanTich { get; set; }

    public virtual DonUngTuyen? MaDonNavigation { get; set; }
}
