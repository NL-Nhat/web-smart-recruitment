using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class ChiTietKyNangUngVien
{
    public int MaUngVien { get; set; }

    public int MaKyNang { get; set; }

    public int? SoNamKinhNghiem { get; set; }

    public virtual DanhMucKyNang MaKyNangNavigation { get; set; } = null!;

    public virtual UngVien MaUngVienNavigation { get; set; } = null!;
}
