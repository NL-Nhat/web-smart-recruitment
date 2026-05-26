namespace web_smart_recruitment.Enums
{
    /// <summary>
    /// Các trạng thái của tin tuyển dụng (TinTuyenDung.TrangThai).
    /// Giá trị string tương ứng được lưu trong database.
    /// </summary>
    public static class TrangThaiTin
    {
        /// <summary>Tin đang hiển thị công khai, nhận hồ sơ ứng tuyển</summary>
        public const string DangMo = "DangMo";

        /// <summary>Tin đã đóng, không nhận thêm hồ sơ</summary>
        public const string DaDong = "DaDong";
    }
}
