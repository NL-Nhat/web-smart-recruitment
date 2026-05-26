namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng khóa/mở khóa tài khoản người dùng (Admin)</summary>
    public class ToggleStatusRequest
    {
        public int MaTaiKhoan { get; set; }
    }
}
