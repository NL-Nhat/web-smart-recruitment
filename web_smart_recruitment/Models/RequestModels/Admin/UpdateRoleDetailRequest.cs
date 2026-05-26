namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng cập nhật thông tin vai trò (Admin)</summary>
    public class UpdateRoleDetailRequest
    {
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}
