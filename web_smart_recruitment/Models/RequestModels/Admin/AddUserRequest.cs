namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng thêm người dùng mới (Admin)</summary>
    public class AddUserRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string VaiTro { get; set; } = null!;

        // Thông tin bổ sung nếu vai trò là Ứng viên
        public string? HoTenCandidate { get; set; }
        public string? SoDienThoaiCandidate { get; set; }

        // Thông tin bổ sung nếu vai trò là Nhà tuyển dụng
        public string? TenCongTy { get; set; }
        public string? SoDienThoaiEmployer { get; set; }
        public string? DiaChi { get; set; }
    }
}
