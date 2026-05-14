using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class LichHenPhongVan
{
    public int MaLichHen { get; set; }

    public int? MaDon { get; set; }

    public DateOnly NgayPhuongVan { get; set; }

    public TimeOnly GioPhuongVan { get; set; }

    public string? DiaDiem { get; set; }

    public string? LinkHop { get; set; }

    public string? HinhThuc { get; set; }

    public string? GhiChu { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual DonUngTuyen? MaDonNavigation { get; set; }
}
