namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng thêm vai trò mới (Admin)</summary>
    public class AddRoleRequest
    {
        public string TenVaiTro { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}
