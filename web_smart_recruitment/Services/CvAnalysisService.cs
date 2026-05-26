using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using web_smart_recruitment.Models;
using web_smart_recruitment.Models.Dtos;
using web_smart_recruitment.Enums;

namespace web_smart_recruitment.Services
{
    // =========================================================================
    // DỊCH VỤ PHÂN TÍCH CV - ICvAnalysisService
    // Interface định nghĩa các hành động mà CvAnalysisService cần thực hiện.
    // =========================================================================
    public interface ICvAnalysisService
    {
        /// <summary>
        /// Phương thức chính: Thực hiện toàn bộ quy trình phân tích CV theo 3 bước.
        /// Được gọi sau khi ứng viên nộp đơn thành công.
        /// </summary>
        /// <param name="maDon">Mã đơn ứng tuyển vừa được tạo</param>
        Task AnalyzeCvAsync(int maDon);
    }

    // =========================================================================
    // TRIỂN KHAI DỊCH VỤ PHÂN TÍCH CV
    // Lớp này thực hiện toàn bộ quy trình phân tích CV gồm 3 bước:
    // Bước 1: Trích xuất văn bản từ file PDF
    // Bước 2: Gọi Gemini AI để phân tích CV so với JD
    // Bước 3: Lưu kết quả phân tích vào database
    // =========================================================================
    public class CvAnalysisService : ICvAnalysisService
    {
        // Các dependency được inject qua constructor
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvAnalysisService> _logger;

        public CvAnalysisService(
            AppDbContext context,
            IWebHostEnvironment env,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<CvAnalysisService> logger)
        {
            _context = context;
            _env = env;
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        // ======================================================================
        // PHƯƠNG THỨC CHÍNH: Điều phối toàn bộ quy trình phân tích
        // ======================================================================
        public async Task AnalyzeCvAsync(int maDon)
        {
            _logger.LogInformation("=== BẮT ĐẦU PHÂN TÍCH CV: MaDon={MaDon} ===", maDon);

            // --- Lấy dữ liệu đơn ứng tuyển, CV, và tin tuyển dụng ---
            // Dùng Include để load dữ liệu liên quan trong 1 truy vấn (tránh N+1 query)
            var don = await _context.DonUngTuyens
                .Include(d => d.MaTinNavigation)          // Load thông tin TinTuyenDung
                    .ThenInclude(t => t!.ChiTietKyNangTinTuyenDungs)  // Load kỹ năng yêu cầu
                        .ThenInclude(c => c.MaKyNangNavigation)       // Load tên kỹ năng
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (don == null)
            {
                _logger.LogWarning("Không tìm thấy đơn ứng tuyển MaDon={MaDon}", maDon);
                return;
            }

            // Kiểm tra xem CV có tồn tại không
            if (string.IsNullOrEmpty(don.DuongDanFile))
            {
                _logger.LogWarning("Đơn MaDon={MaDon} không có file CV", maDon);
                return;
            }

            // ================================================================
            // BƯỚC 1: TRÍCH XUẤT VĂN BẢN TỪ FILE PDF
            // ================================================================
            string extractedCvText = string.Empty;
            try
            {
                extractedCvText = await Step1_ExtractTextFromPdfAsync(don);
                _logger.LogInformation("Bước 1 hoàn thành: Trích xuất {Length} ký tự từ CV", extractedCvText.Length);
            }
            catch (Exception ex)
            {
                string errMsg = $"[Bước 1 THẤT BẠI] Không trích xuất được PDF: {ex.Message}";
                _logger.LogError(ex, errMsg);
                await SetKetQuaLoi(maDon, errMsg);
                return;
            }

            // ================================================================
            // BƯỚC 2: GỌI GEMINI AI ĐỂ PHÂN TÍCH CV
            // ================================================================
            string? rawJsonFromAI = null;
            try
            {
                rawJsonFromAI = await Step2_CallGeminiApiAsync(extractedCvText, don.MaTinNavigation!);
                _logger.LogInformation("Bước 2 hoàn thành: Nhận được phản hồi JSON từ Gemini AI");
            }
            catch (Exception ex)
            {
                string errMsg = $"[Bước 2 THẤT BẠI] Lỗi gọi Gemini API: {ex.Message}";
                _logger.LogError(ex, errMsg);
                await SetKetQuaLoi(maDon, errMsg);
                return;
            }

            // ================================================================
            // BƯỚC 3: PHÂN TÍCH KẾT QUẢ JSON VÀ LƯU VÀO DATABASE
            // ================================================================
            try
            {
                await Step3_ParseAndSaveResultAsync(maDon, rawJsonFromAI!);
                _logger.LogInformation("=== HOÀN THÀNH PHÂN TÍCH CV: MaDon={MaDon} ===", maDon);
            }
            catch (Exception ex)
            {
                string errMsg = $"[Bước 3 THẤT BẠI] Lỗi lưu kết quả AI: {ex.Message}";
                _logger.LogError(ex, errMsg);
                await SetKetQuaLoi(maDon, errMsg);
            }
        }

        // ======================================================================
        // BƯỚC 1: TRÍCH XUẤT VĂN BẢN TỪ FILE PDF
        // Sử dụng thư viện UglyToad.PdfPig để đọc từng trang PDF
        // và gom toàn bộ chữ lại thành 1 chuỗi dài (extractedCvText)
        // ======================================================================
        private async Task<string> Step1_ExtractTextFromPdfAsync(DonUngTuyen hoSo)
        {
            // Dựng đường dẫn tuyệt đối đến file trên server
            // DuongDanFile lưu dạng "/uploads/cvs/tên_file.pdf"
            string absolutePath = Path.Combine(_env.WebRootPath, hoSo.DuongDanFile.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            _logger.LogInformation("Đang đọc file PDF: {Path}", absolutePath);

            if (!File.Exists(absolutePath))
                throw new FileNotFoundException($"Không tìm thấy file CV tại: {absolutePath}");

            // Dùng StringBuilder để ghép nối chuỗi hiệu quả hơn string cộng thông thường
            var textBuilder = new StringBuilder();

            // PdfDocument.Open() mở file PDF để đọc
            using (var pdfDoc = PdfDocument.Open(absolutePath))
            {
                // Duyệt qua từng trang trong file PDF
                foreach (var page in pdfDoc.GetPages())
                {
                    // GetWords() lấy tất cả các từ trên trang
                    // Ghép các từ lại thành 1 chuỗi, phân cách bằng khoảng trắng
                    textBuilder.AppendLine(string.Join(" ", page.GetWords().Select(w => w.Text)));
                }
            }

            string extractedText = textBuilder.ToString();

            // Lưu văn bản trích xuất vào cột NoiDungTrichXuat trong bảng HoSoCV
            hoSo.NoiDungTrichXuat = extractedText;
            await _context.SaveChangesAsync(); // Lưu lại ngay vào DB

            return extractedText;
        }

        // ======================================================================
        // BƯỚC 2: GỌI GEMINI AI ĐỂ PHÂN TÍCH CV SO VỚI JD
        // Có Retry Logic: Tự động thử lại tối đa N lần nếu gặp lỗi
        // ======================================================================
        private async Task<string> Step2_CallGeminiApiAsync(string cvText, TinTuyenDung tin)
        {
            // Đọc cấu hình API từ appsettings.json
            string apiKey  = _config["GeminiApi:ApiKey"] ?? throw new InvalidOperationException("Chưa cấu hình GeminiApi:ApiKey");
            string model   = _config["GeminiApi:ModelName"] ?? "gemini-1.5-flash";
            int maxRetries = int.Parse(_config["GeminiApi:MaxRetries"] ?? "3");

            // Kiểm tra nhanh: không tiếp tục nếu API key còn là placeholder
            if (apiKey == "YOUR_GEMINI_API_KEY_HERE")
                throw new InvalidOperationException("Vui lòng cấu hình GeminiApi:ApiKey thực trong appsettings.json");

            // === CHUẨN BỊ DỮ LIỆU JD (Job Description) ===
            var kyNangText = new StringBuilder();
            foreach (var chiTiet in tin.ChiTietKyNangTinTuyenDungs)
            {
                kyNangText.AppendLine($"- {chiTiet.MaKyNangNavigation?.TenKyNang} ({chiTiet.CapDoYeuCau})");
            }

            // === XÂY DỰNG PROMPT ===
            string cvTextTruncated = cvText.Substring(0, Math.Min(cvText.Length, 6000));

            // Template mẫu với giá trị cụ thể (không dùng placeholder) để AI hiểu rõ hơn
            // Yêu cầu AI trả lời NGẮN GỌN để tránh bị cắt token
            string jsonTemplate =
                "{\n" +
                "  \"DiemPhuHop\": 85.5,\n" +
                "  \"TomTatUngVien\": \"Toi da la 2-3 cau ngan.\",\n" +
                "  \"KyNangPhuHop\": [\"C#\", \"SQL\"],\n" +
                "  \"KyNangThieu\": [\"Docker\"],\n" +
                "  \"DiemManh\": \"Mot cau ngan.\",\n" +
                "  \"DiemYeu\": \"Mot cau ngan.\",\n" +
                "  \"DeXuat\": \"TuyenNhanh\"\n" +
                "}";

            // Prompt Engineering: Yêu cầu trả lời NGẮN GỌN để tránh bị cắt do giới hạn token
            string prompt =
                "Bạn là HR. Phân tích CV dưới đây và trả về JSON ngắn gọn theo mẫu.\n\n" +
                "=== JD ===\n" +
                $"Vị trí: {tin.TieuDe}\n" +
                $"Yêu cầu: {tin.YeuCauCongViec?.Substring(0, Math.Min(tin.YeuCauCongViec?.Length ?? 0, 1000))}\n" +
                $"Kỹ năng:\n{kyNangText}\n" +
                "=== CV ===\n" +
                cvTextTruncated + "\n\n" +
                "=== YÊU CẦU ===\n" +
                "Chỉ trả về JSON thuần túy (KHÔNG markdown, KHÔNG giải thích). " +
                "Mỗi trường string CHỈ ĐƯỢC TỐI ĐA 100 ký tự. " +
                "Mảng kỹ năng tối đa 8 phần tử. Cấu trúc:\n" +
                jsonTemplate + "\n" +
                "Lưu ý: DeXuat chỉ là: TuyenNhanh, CoTheCanNhac, hoặc LoaiBo";

            // ================================================================
            // BẢO MẬT API KEY: Dùng HTTP Header thay vì URL Query String
            //
            // SAI (key bị log): ?key=xxx trong URL → bị HttpClient log ra Output
            // ĐÚNG: Gửi key qua header x-goog-api-key → KHÔNG bị log URL
            // ================================================================
            // URL không chứa key → URL này an toàn để log
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

            string lastError = "";

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                _logger.LogInformation("Gọi Gemini API: Lần thử {Attempt}/{MaxRetries}", attempt, maxRetries);

                try
                {
                    // ================================================================
                    // LƯU Ý QUAN TRỌNG về Gemini 2.5-flash:
                    // Đây là "thinking model" - nó dùng nhiều token để "suy nghĩ" trước khi trả lời.
                    // Nếu không tắt thinking, maxOutputTokens bị ăn bởi internal thoughts,
                    // dẫn đến JSON trả về bị cắt ngắn (MAX_TOKENS).
                    //
                    // Giải pháp: Thêm thinkingConfig.thinkingBudget = 0 để tắt hoàn toàn thinking.
                    // Kết quả: 100% token dành cho JSON output thực tế.
                    // ================================================================
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.1,
                            maxOutputTokens = 4096,     // Đủ lớn cho JSON đầy đủ
                            thinkingConfig = new
                            {
                                // thinkingBudget = 0: Tắt hoàn toàn thinking mode
                                // → Tiết kiệm token, JSON không bị cắt
                                // Áp dụng cho: gemini-2.5-flash, gemini-2.5-pro
                                // Bỏ qua tự động với các model cũ hơn
                                thinkingBudget = 0
                            }
                        }
                    };

                    string requestJson = JsonSerializer.Serialize(requestBody);
                    var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    // ============================================================
                    // GỬI KEY QUA HTTP HEADER (KHÔNG qua URL query string)
                    // Cách này: URL được log KHÔNG chứa key → an toàn
                    // Google Gemini hỗ trợ header: x-goog-api-key
                    // ============================================================
                    using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("x-goog-api-key", apiKey); // Key truyền qua header
                    request.Content = httpContent;

                    // Đặt timeout 60 giây để tránh treo vô tận
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60));
                    var response = await _httpClient.SendAsync(request, cts.Token);

                    // Đọc toàn bộ response để log debug
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Log response để dễ debug
                    _logger.LogDebug("Gemini response (attempt {Attempt}): {Body}",
                        attempt, responseBody.Substring(0, Math.Min(responseBody.Length, 500)));

                    if (!response.IsSuccessStatusCode)
                    {
                        lastError = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                        _logger.LogWarning("Lần thử {Attempt}: Gemini API lỗi HTTP {Status} - {Error}",
                            attempt, response.StatusCode, lastError);
                        if (attempt < maxRetries) await Task.Delay(2000 * attempt); // Backoff tăng dần
                        continue;
                    }

                    // === PARSE RESPONSE CỦA GEMINI ===
                    // Cấu trúc chuẩn: { "candidates": [{ "content": { "parts": [{ "text": "...json..." }] } }] }
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    // Kiểm tra candidates có tồn tại không
                    if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    {
                        lastError = "Gemini không trả về candidates. Response: " + responseBody.Substring(0, Math.Min(responseBody.Length, 300));
                        _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                        if (attempt < maxRetries) await Task.Delay(2000 * attempt);
                        continue;
                    }

                    var firstCandidate = candidates[0];

                    // Kiểm tra finishReason - Gemini có thể từ chối do safety filter
                    if (firstCandidate.TryGetProperty("finishReason", out var finishReason))
                    {
                        string reason = finishReason.GetString() ?? "";
                        if (reason == "SAFETY" || reason == "RECITATION" || reason == "OTHER")
                        {
                            lastError = $"Gemini từ chối trả lời do: {reason}";
                            _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                            if (attempt < maxRetries) await Task.Delay(2000 * attempt);
                            continue;
                        }
                    }

                    // Lấy nội dung text từ phần đầu tiên
                    if (!firstCandidate.TryGetProperty("content", out var content) ||
                        !content.TryGetProperty("parts", out var parts) ||
                        parts.GetArrayLength() == 0)
                    {
                        lastError = "Không tìm thấy content.parts trong response của Gemini";
                        _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                        if (attempt < maxRetries) await Task.Delay(2000 * attempt);
                        continue;
                    }

                    string rawText = parts[0].GetProperty("text").GetString() ?? string.Empty;
                    rawText = rawText.Trim();

                    // Kiểm tra finishReason = MAX_TOKENS (JSON bị cắt do hết quota token)
                    // Log cảnh báo nhưng vẫn thử xử lý
                    if (firstCandidate.TryGetProperty("finishReason", out var finishReason2))
                    {
                        string reason2 = finishReason2.GetString() ?? "";
                        if (reason2 == "MAX_TOKENS")
                        {
                            _logger.LogWarning("Lần thử {Attempt}: Gemini trả về MAX_TOKENS - JSON có thể bị cắt. Độ dài text: {Len}", attempt, rawText.Length);
                        }
                    }

                    // Log TOÀN BỘ raw text để debug (không bị cắt như trước)
                    _logger.LogInformation("=== FULL RAW TEXT (Lần {Attempt}) ===\n{Text}", attempt, rawText);

                    // Loại bỏ markdown wrapper nếu AI vẫn trả về ```json ... ```
                    if (rawText.StartsWith("```"))
                    {
                        int jsonStart = rawText.IndexOf('{');
                        int jsonEnd = rawText.LastIndexOf('}');
                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            rawText = rawText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        }
                        else
                        {
                            rawText = rawText.Replace("```json", "").Replace("```", "").Trim();
                        }
                    }

                    // === XỬ LÝ JSON BỊ CẮT NGẮN ===
                    // Nếu JSON không kết thúc bằng '}', cố gắng tìm '}' cuối cùng
                    // và lấy phần hợp lệ (bỏ phần bị cắt)
                    if (!rawText.EndsWith("}"))
                    {
                        int lastBrace = rawText.LastIndexOf('}');
                        if (lastBrace > 0)
                        {
                            _logger.LogWarning("Lần thử {Attempt}: JSON có vẻ bị cắt, thử cắt tại '}}' cuối cùng (pos={Pos})", attempt, lastBrace);
                            rawText = rawText.Substring(0, lastBrace + 1);
                        }
                    }

                    // Kiểm tra JSON hợp lệ trước khi trả về
                    using var testParse = JsonDocument.Parse(rawText);
                    _logger.LogInformation("Bước 2 thành công ở lần thử {Attempt}", attempt);
                    return rawText;
                }
                catch (JsonException jsonEx)
                {
                    // AI trả về text không phải JSON hợp lệ
                    lastError = $"JSON không hợp lệ: {jsonEx.Message}";
                    _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                    if (attempt < maxRetries) await Task.Delay(2000 * attempt);
                }
                catch (TaskCanceledException)
                {
                    // Timeout sau 60 giây
                    lastError = "Timeout: Gemini không phản hồi trong 60 giây";
                    _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                    if (attempt < maxRetries) await Task.Delay(3000);
                }
                catch (HttpRequestException httpEx)
                {
                    // Lỗi kết nối mạng
                    lastError = $"Lỗi kết nối: {httpEx.Message}";
                    _logger.LogWarning("Lần thử {Attempt}: {Error}", attempt, lastError);
                    if (attempt < maxRetries) await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    // Bắt tất cả lỗi khác (InvalidOperationException từ GetProperty, v.v.)
                    lastError = $"Lỗi không xác định: {ex.Message}";
                    _logger.LogError(ex, "Lần thử {Attempt}: Lỗi khi xử lý response Gemini", attempt);
                    if (attempt < maxRetries) await Task.Delay(2000 * attempt);
                }
            }

            // Đã thử hết số lần mà vẫn thất bại
            throw new InvalidOperationException(
                $"Không thể nhận phản hồi hợp lệ từ Gemini AI sau {maxRetries} lần thử. Lỗi cuối: {lastError}");
        }

        // ======================================================================
        // BƯỚC 3: PARSE KẾT QUẢ JSON VÀ LƯU VÀO BẢNG KetQua_AI
        // ======================================================================
        private async Task Step3_ParseAndSaveResultAsync(int maDon, string rawJsonFromAI)
        {
            // === ép kiểu chuỗi JSON thành đối tượng C# ===
            // AiResponseDto là class chứa cấu trúc dữ liệu mà AI trả về
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Bỏ qua chữ hoa/thường của property
            };
            var aiResult = JsonSerializer.Deserialize<AiResponseDto>(rawJsonFromAI, options)
                           ?? throw new InvalidOperationException("Không thể deserialize kết quả AI");

            // Serialize lại mảng kỹ năng thành chuỗi JSON để lưu vào DB
            // Ví dụ: ["C#", "ASP.NET Core"]
            string kyNangPhuHopJson = JsonSerializer.Serialize(aiResult.KyNangPhuHop ?? new List<string>());
            string kyNangThieuJson  = JsonSerializer.Serialize(aiResult.KyNangThieu ?? new List<string>());

            // === KIỂM TRA XEM ĐÃ CÓ BẢN GHI KetQua_AI CHO ĐƠN NÀY CHƯA ===
            // Tránh tạo bản ghi trùng lặp nếu hàm bị gọi lại
            var existing = await _context.KetQuaAis.FirstOrDefaultAsync(k => k.MaDon == maDon);

            if (existing != null)
            {
                // Cập nhật bản ghi đã có (trường hợp gọi lại sau khi thất bại)
                existing.TrangThaiXuLy    = TrangThaiKetQuaAi.HoanThanh;
                existing.DiemPhuHop       = (decimal?)aiResult.DiemPhuHop;
                existing.TomTatUngVien    = aiResult.TomTatUngVien;
                existing.KyNangPhuHopJson = kyNangPhuHopJson;
                existing.KyNangThieuJson  = kyNangThieuJson;
                existing.DiemManh         = aiResult.DiemManh;
                existing.DiemYeu          = aiResult.DiemYeu;
                existing.DeXuat           = aiResult.DeXuat;
                existing.PhanHoiGocTuAi   = rawJsonFromAI; // Lưu raw JSON để debug
                existing.NgayPhanTich     = DateTime.Now;
            }
            else
            {
                // Tạo bản ghi mới trong bảng KetQua_AI
                _context.KetQuaAis.Add(new KetQuaAi
                {
                    MaDon             = maDon,
                    TrangThaiXuLy     = TrangThaiKetQuaAi.HoanThanh,
                    DiemPhuHop        = (decimal?)aiResult.DiemPhuHop,
                    TomTatUngVien     = aiResult.TomTatUngVien,
                    KyNangPhuHopJson  = kyNangPhuHopJson,
                    KyNangThieuJson   = kyNangThieuJson,
                    DiemManh          = aiResult.DiemManh,
                    DiemYeu           = aiResult.DiemYeu,
                    DeXuat            = aiResult.DeXuat,
                    PhanHoiGocTuAi    = rawJsonFromAI, // Lưu raw JSON để debug
                    NgayPhanTich      = DateTime.Now
                });
            }

            // Cập nhật trạng thái đơn ứng tuyển sang AIDaLoc (AI đã lọc xong)
            var don = await _context.DonUngTuyens.FindAsync(maDon);
            if (don != null)
            {
                don.TrangThai   = TrangThaiDon.AIDaLoc;
                don.NgayCapNhat = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        // ======================================================================
        // PHƯƠNG THỨC PHỤ: Đặt trạng thái lỗi và lưu message lỗi để debug
        // ======================================================================
        private async Task SetKetQuaLoi(int maDon, string errorMessage = "Lỗi không xác định")
        {
            // Kiểm tra xem đã có bản ghi chưa (đã được tạo từ Controller với status DangXuLy)
            var existing = await _context.KetQuaAis.FirstOrDefaultAsync(k => k.MaDon == maDon);
            if (existing != null)
            {
                existing.TrangThaiXuLy  = TrangThaiKetQuaAi.Loi;
                existing.PhanHoiGocTuAi = errorMessage; // Lưu chi tiết lỗi để debug
                existing.NgayPhanTich   = DateTime.Now;
            }
            else
            {
                _context.KetQuaAis.Add(new KetQuaAi
                {
                    MaDon           = maDon,
                    TrangThaiXuLy   = TrangThaiKetQuaAi.Loi,
                    PhanHoiGocTuAi  = errorMessage, // Lưu chi tiết lỗi để debug
                    NgayPhanTich    = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();
        }
    }

    // AiResponseDto đã được chuyển sang Models/Dtos/AiResponseDto.cs
}
