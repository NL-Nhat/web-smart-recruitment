namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các trạng thái của lịch hẹn phỏng vấn (LichHenPhongVan.TrangThai).
    /// Giá trị string tương ứng được lưu trong database.
    /// </summary>
    public static class TrangThaiLichHen
    {
        /// <summary>HR đã tạo lịch hẹn, đang chờ ứng viên xác nhận</summary>
        public const string ChoXacNhan = "ChoXacNhan";

        /// <summary>Ứng viên đã xác nhận tham gia phỏng vấn</summary>
        public const string DaXacNhan = "DaXacNhan";

        /// <summary>Lịch hẹn đã bị hủy (HR hủy hoặc ứng viên từ chối)</summary>
        public const string DaHuy = "DaHuy";

        /// <summary>Buổi phỏng vấn đã diễn ra xong</summary>
        public const string HoanThanh = "HoanThanh";
    }
}
