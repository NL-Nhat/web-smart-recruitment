namespace web_smart_recruitment.Models.Dtos
{
    /// <summary>
    /// DTO đại diện cho một hoạt động gần đây trong Dashboard của HR.
    /// Dùng để gộp các sự kiện (nộp đơn, AI xong, lịch hẹn) vào một danh sách duy nhất.
    /// </summary>
    public class HrActivityDto
    {
        /// <summary>Thời điểm xảy ra hoạt động</summary>
        public DateTime Time { get; set; }

        /// <summary>Tiêu đề ngắn hiển thị trên timeline (ví dụ: tên ứng viên, "AI Analysis Hoàn tất")</summary>
        public string Title { get; set; } = null!;

        /// <summary>Mô tả chi tiết hoạt động</summary>
        public string Description { get; set; } = null!;

        /// <summary>HTML của SVG icon hiển thị bên trái mỗi dòng activity</summary>
        public string IconHtml { get; set; } = null!;

        /// <summary>Inline CSS color cho icon (ví dụ: "color: var(--el-success);")</summary>
        public string IconColor { get; set; } = null!;
    }
}
