namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng thêm kỹ năng mới vào danh mục (Admin)</summary>
    public class AddSkillRequest
    {
        public string TenKyNang { get; set; } = null!;
        public string? PhanLoai { get; set; }
    }
}
