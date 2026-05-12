using System;
using System.Collections.Generic;

namespace web_smart_recruitment.Models;

public partial class ChiTietKyNangTinTuyenDung
{
    public int MaTin { get; set; }

    public int MaKyNang { get; set; }

    public string? CapDoYeuCau { get; set; }

    public virtual DanhMucKyNang MaKyNangNavigation { get; set; } = null!;

    public virtual TinTuyenDung MaTinNavigation { get; set; } = null!;
}
