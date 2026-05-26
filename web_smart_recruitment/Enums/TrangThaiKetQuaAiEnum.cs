namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các trạng thái xử lý của kết quả phân tích AI (KetQuaAi.TrangThaiXuLy).
    /// Giá trị string tương ứng được lưu trong database.
    /// </summary>
    public static class TrangThaiKetQuaAi
    {
        /// <summary>AI đang trong quá trình phân tích (Background Service chưa xong)</summary>
        public const string DangXuLy = "DangXuLy";

        /// <summary>AI đã phân tích thành công, có đầy đủ kết quả</summary>
        public const string HoanThanh = "HoanThanh";

        /// <summary>AI gặp lỗi trong quá trình phân tích, xem PhanHoiGocTuAi để biết chi tiết</summary>
        public const string Loi = "Loi";
    }
}
