namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các trạng thái của đơn ứng tuyển (DonUngTuyen.TrangThai).
    /// Giá trị string tương ứng được lưu trong database.
    /// </summary>
    public static class TrangThaiDon
    {
        /// <summary>Ứng viên vừa nộp đơn, chưa được xử lý</summary>
        public const string DaNop = "DaNop";

        /// <summary>AI đã phân tích xong CV</summary>
        public const string AIDaLoc = "AIDaLoc";

        /// <summary>HR đã xem và chấp nhận hồ sơ, chờ xếp lịch phỏng vấn</summary>
        public const string DaChapNhan = "DaChapNhan";

        /// <summary>Đang ở giai đoạn phỏng vấn</summary>
        public const string PhongVan = "PhongVan";

        /// <summary>HR đã từ chối hồ sơ</summary>
        public const string TuChoi = "TuChoi";

        /// <summary>Ứng viên đã trúng tuyển</summary>
        public const string TrungTuyen = "TrungTuyen";
    }
}
