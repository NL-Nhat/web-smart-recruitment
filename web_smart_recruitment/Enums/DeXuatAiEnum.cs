namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các giá trị đề xuất từ AI sau khi phân tích CV (KetQuaAi.DeXuat).
    /// Giá trị string tương ứng được lưu trong database.
    /// </summary>
    public static class DeXuatAi
    {
        /// <summary>AI đề xuất tuyển ngay — ứng viên rất phù hợp (thường DiemPhuHop >= 80%)</summary>
        public const string TuyenNhanh = "TuyenNhanh";

        /// <summary>AI đề xuất cân nhắc thêm — ứng viên khá phù hợp (thường DiemPhuHop 50-79%)</summary>
        public const string CoTheCanNhac = "CoTheCanNhac";

        /// <summary>AI đề xuất loại bỏ — ứng viên không phù hợp (thường DiemPhuHop < 50%)</summary>
        public const string LoaiBo = "LoaiBo";
    }
}
