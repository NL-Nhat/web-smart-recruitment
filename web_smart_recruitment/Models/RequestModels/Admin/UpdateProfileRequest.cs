namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng cập nhật hồ sơ Admin (email và mật khẩu)</summary>
    public class UpdateProfileRequest
    {
        public string Email { get; set; } = null!;
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
