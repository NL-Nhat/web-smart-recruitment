namespace web_smart_recruitment.Models.RequestModels.Admin
{
    /// <summary>Request model cho chức năng cập nhật kỹ năng trong danh mục (Admin)</summary>
    public class UpdateSkillRequest
    {
        public int MaKyNang { get; set; }
        public string TenKyNang { get; set; } = null!;
        public string? PhanLoai { get; set; }
    }
}
