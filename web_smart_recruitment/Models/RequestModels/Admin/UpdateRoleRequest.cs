namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng cập nhật vai trò của người dùng (Admin)</summary>
    public class UpdateRoleRequest
    {
        public int MaTaiKhoan { get; set; }
        public string TenVaiTro { get; set; } = null!;
    }
}
