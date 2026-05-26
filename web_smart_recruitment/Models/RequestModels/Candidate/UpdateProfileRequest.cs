namespace web_smart_recruitment.Models.RequestModels.Candidate
{
    /// <summary>Request model cho chức năng cập nhật hồ sơ cá nhân của ứng viên</summary>
    public class UpdateProfileRequest
    {
        public string HoTen { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public int? SoNamKinhNghiem { get; set; }
        public string? LinkLinkedIn { get; set; }
        public string? ChucDanhHienTai { get; set; }
    }
}
