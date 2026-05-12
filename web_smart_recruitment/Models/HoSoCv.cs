using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class HoSoCv
{
    public int MaCv { get; set; }

    public int? MaUngVien { get; set; }

    public string TenFile { get; set; } = null!;

    public string DuongDanFile { get; set; } = null!;

    public string DinhDang { get; set; } = null!;

    public string? NoiDungTrichXuat { get; set; }

    public DateTime? NgayTaiLen { get; set; }

    public virtual ICollection<DonUngTuyen> DonUngTuyens { get; set; } = new List<DonUngTuyen>();

    public virtual UngVien? MaUngVienNavigation { get; set; }
}
