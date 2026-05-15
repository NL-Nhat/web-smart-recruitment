using web_smart_recruitment.Models;

namespace web_smart_recruitment.Services
{
    // =========================================================================
    // BACKGROUND SERVICE - CHẠY NGẦM ĐỂ XỬ LÝ TÁC VỤ NẶNG
    //
    // Tại sao cần Background Service?
    // - Phân tích CV là tác vụ nặng (đọc PDF + gọi AI API có thể mất 5-30 giây)
    // - Nếu làm trực tiếp trong Controller, người dùng phải đứng chờ rất lâu
    // - Background Service giúp: Controller trả về response ngay lập tức,
    //   trong khi việc phân tích CV chạy ngầm phía sau.
    //
    // Kiến trúc hoạt động:
    // [Controller] --> Đẩy MaDon vào Queue --> Trả về "Đã nhận đơn" cho user
    //                                  |
    //                                  v
    //         [CvAnalysisBackgroundService] (chạy ngầm)
    //                 --> Lấy MaDon từ Queue
    //                 --> Gọi CvAnalysisService.AnalyzeCvAsync()
    //                 --> Lưu kết quả vào DB
    // =========================================================================

    /// <summary>
    /// Channel Queue dùng để truyền thông tin giữa Controller và Background Service.
    /// Singleton: Chỉ có 1 instance duy nhất trong toàn bộ ứng dụng.
    /// </summary>
    public class CvAnalysisQueue
    {
        // System.Threading.Channels là hàng đợi thread-safe của .NET (không cần lock thủ công)
        // Capacity = 100: Hàng đợi chứa tối đa 100 đơn đang chờ xử lý
        private readonly System.Threading.Channels.Channel<int> _channel =
            System.Threading.Channels.Channel.CreateBounded<int>(capacity: 100);

        /// <summary>
        /// Controller gọi hàm này để đẩy MaDon vào hàng đợi sau khi nộp đơn thành công
        /// </summary>
        public async Task EnqueueAsync(int maDon) =>
            await _channel.Writer.WriteAsync(maDon);

        /// <summary>
        /// Background Service gọi hàm này để đọc MaDon từ hàng đợi (sẽ chờ nếu hàng đợi rỗng)
        /// </summary>
        public IAsyncEnumerable<int> DequeueAllAsync(CancellationToken ct) =>
            _channel.Reader.ReadAllAsync(ct);
    }

    // =========================================================================
    // BACKGROUND SERVICE THỰC THI
    // IHostedService: Interface của .NET để tạo service chạy ngầm vòng lặp vô tận
    // =========================================================================
    public class CvAnalysisBackgroundService : BackgroundService
    {
        private readonly CvAnalysisQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CvAnalysisBackgroundService> _logger;

        public CvAnalysisBackgroundService(
            CvAnalysisQueue queue,
            IServiceScopeFactory scopeFactory,  // Dùng ScopeFactory vì CvAnalysisService là Scoped
            ILogger<CvAnalysisBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // ======================================================================
        // VÒNG LẶP CHÍNH: Chờ và xử lý các đơn ứng tuyển trong hàng đợi
        // Hàm này chạy vô tận từ khi ứng dụng khởi động đến khi tắt
        // ======================================================================
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CvAnalysisBackgroundService đã KHỞI ĐỘNG và đang chờ...");

            // Duyệt qua tất cả các MaDon trong hàng đợi (await foreach sẽ chờ nếu rỗng)
            await foreach (int maDon in _queue.DequeueAllAsync(stoppingToken))
            {
                _logger.LogInformation("Nhận được yêu cầu phân tích CV cho MaDon={MaDon}", maDon);

                try
                {
                    // Tạo một Scope mới vì CvAnalysisService (Scoped) không thể dùng trực tiếp
                    // trong Singleton BackgroundService - đây là pattern chuẩn của .NET DI
                    using var scope = _scopeFactory.CreateScope();
                    var analysisService = scope.ServiceProvider.GetRequiredService<ICvAnalysisService>();

                    // Gọi dịch vụ phân tích - đây là hàm async chạy 3 bước
                    await analysisService.AnalyzeCvAsync(maDon);
                }
                catch (Exception ex)
                {
                    // Lỗi không nên làm crash toàn bộ Background Service
                    // Log lỗi và tiếp tục xử lý đơn tiếp theo
                    _logger.LogError(ex, "Lỗi khi phân tích CV cho MaDon={MaDon}", maDon);
                }
            }

            _logger.LogInformation("CvAnalysisBackgroundService đã DỪNG.");
        }
    }
}
