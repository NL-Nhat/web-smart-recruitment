namespace web_smart_recruitment.Models.Dtos
{
    /// <summary>
    /// DTO đại diện cho một hoạt động trong báo cáo tổng hợp của Admin.
    /// Dùng để gộp tin tuyển dụng, lượt ứng tuyển và người dùng mới vào một danh sách duy nhất.
    /// Hỗ trợ cả export CSV và hiển thị trên trang báo cáo.
    /// </summary>
    public class ActivityLogDto
    {
        /// <summary>Thời điểm xảy ra hoạt động (có thể null nếu chưa có ngày)</summary>
        public DateTime? Time { get; set; }

        /// <summary>Loại dữ liệu (ví dụ: "TIN TUYỂN DỤNG", "ỨNG TUYỂN", "NGƯỜI DÙNG")</summary>
        public string Type { get; set; } = null!;

        /// <summary>Nội dung mô tả hoạt động</summary>
        public string Content { get; set; } = null!;

        /// <summary>Người/tổ chức thực hiện hoạt động</summary>
        public string Actor { get; set; } = null!;

        /// <summary>CSS class dùng để tô màu badge trạng thái trên UI (ví dụ: "el-status--info")</summary>
        public string CssClass { get; set; } = string.Empty;
    }
}
