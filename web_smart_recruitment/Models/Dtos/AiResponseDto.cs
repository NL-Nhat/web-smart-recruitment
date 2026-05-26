using System.Text.Json.Serialization;

namespace web_smart_recruitment.Models.Dtos
{
    /// <summary>
    /// DTO ánh xạ (Deserialize) chuỗi JSON từ Gemini AI trả về.
    ///
    /// Cấu trúc JSON mẫu AI trả về:
    /// {
    ///   "DiemPhuHop": 85.5,
    ///   "TomTatUngVien": "Ứng viên có kinh nghiệm 3 năm...",
    ///   "KyNangPhuHop": ["C#", "ASP.NET Core"],
    ///   "KyNangThieu": ["Docker", "Kubernetes"],
    ///   "DiemManh": "Kinh nghiệm thực tế tốt.",
    ///   "DiemYeu": "Chưa có kinh nghiệm DevOps.",
    ///   "DeXuat": "TuyenNhanh"
    /// }
    ///
    /// Lưu ý: Dùng [JsonPropertyName] để map chính xác với key JSON từ AI,
    /// bất kể AI có dùng chữ hoa/thường khác nhau không.
    /// </summary>
    public class AiResponseDto
    {
        /// <summary>Điểm phù hợp tổng thể, thang điểm 0-100</summary>
        [JsonPropertyName("DiemPhuHop")]
        public double DiemPhuHop { get; set; }

        /// <summary>Tóm tắt ngắn gọn về ứng viên (tối đa ~100 ký tự theo prompt)</summary>
        [JsonPropertyName("TomTatUngVien")]
        public string? TomTatUngVien { get; set; }

        /// <summary>Danh sách kỹ năng ứng viên có và khớp với yêu cầu JD</summary>
        [JsonPropertyName("KyNangPhuHop")]
        public List<string>? KyNangPhuHop { get; set; }

        /// <summary>Danh sách kỹ năng JD yêu cầu nhưng ứng viên còn thiếu</summary>
        [JsonPropertyName("KyNangThieu")]
        public List<string>? KyNangThieu { get; set; }

        /// <summary>Nhận xét điểm mạnh cốt lõi của ứng viên</summary>
        [JsonPropertyName("DiemManh")]
        public string? DiemManh { get; set; }

        /// <summary>Nhận xét điểm yếu / cần cải thiện của ứng viên</summary>
        [JsonPropertyName("DiemYeu")]
        public string? DiemYeu { get; set; }

        /// <summary>
        /// Đề xuất tuyển dụng từ AI.
        /// Chỉ nhận 3 giá trị: "TuyenNhanh", "CoTheCanNhac", "LoaiBo"
        /// (xem <see cref="web_smart_recruitment.Enums.DeXuatAi"/>)
        /// </summary>
        [JsonPropertyName("DeXuat")]
        public string? DeXuat { get; set; }
    }
}
