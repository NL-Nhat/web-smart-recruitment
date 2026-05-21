using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class DonUngTuyen
{
    public int MaDon { get; set; }

    public int? MaTin { get; set; }

    public int? MaUngVien { get; set; }

    public string TenFile { get; set; } = null!;

    public string DuongDanFile { get; set; } = null!;

    public string DinhDang { get; set; } = null!;

    public string? NoiDungTrichXuat { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayNop { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<KetQuaAi> KetQuaAis { get; set; } = new List<KetQuaAi>();

    public virtual ICollection<LichHenPhongVan> LichHenPhongVans { get; set; } = new List<LichHenPhongVan>();

    public virtual TinTuyenDung? MaTinNavigation { get; set; }

    public virtual UngVien? MaUngVienNavigation { get; set; }
}
