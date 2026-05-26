namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các hình thức phỏng vấn (LichHenPhongVan.HinhThuc).
    /// Ràng buộc DB: Nếu Online thì LinkHop phải có giá trị, DiaDiem phải NULL.
    ///               Nếu Offline thì DiaDiem phải có giá trị, LinkHop phải NULL.
    /// (CHK constraint: CHK_LichHen_DiaDiem_LinkHop)
    /// </summary>
    public static class HinhThucPhongVan
    {
        /// <summary>Phỏng vấn trực tuyến — yêu cầu LinkHop (Google Meet, Zoom...)</summary>
        public const string Online = "Online";

        /// <summary>Phỏng vấn trực tiếp — yêu cầu DiaDiem (địa chỉ văn phòng...)</summary>
        public const string Offline = "Offline";
    }
}
