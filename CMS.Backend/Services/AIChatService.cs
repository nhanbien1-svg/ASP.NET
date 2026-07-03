using System.Text;
using System.Text.Json;

namespace CMS.Backend.Services
{
    public class AIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIChatService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AIChatService(HttpClient httpClient, IConfiguration configuration, ILogger<AIChatService> logger, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<string> AskGeminiAsync(string userMessage)
        {
            string apiKey = _configuration["GeminiApi:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Hệ thống AI chưa được cấu hình API Key. Vui lòng cập nhật appsettings.json.";
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={apiKey}";

            // Định nghĩa prompt bối cảnh cho Chatbot
            string systemPrompt = @"Bạn là trợ lý ảo tư vấn khách hàng của cửa hàng điện tử TechZone (chuyên bán Laptop, Điện thoại).
Nhiệm vụ của bạn:
- Luôn thân thiện, lịch sự và xưng hô là 'mình' - 'bạn' hoặc 'dạ em' - 'anh/chị'.
- Trả lời ngắn gọn, đúng trọng tâm câu hỏi, không dài dòng.
- Sử dụng tiếng Việt một cách tự nhiên.
- Dưới đây là danh sách CÁC SẢN PHẨM HIỆN CÓ TẠI CỬA HÀNG. Dựa vào danh sách này để báo giá, kiểm tra tồn kho và gợi ý cho khách:
";
            try 
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<CMS.Data.ApplicationDbContext>();
                    var products = db.Products.Where(p => p.IsActive).Select(p => new { p.Name, p.Price, p.StockQuantity }).ToList();
                    foreach (var p in products)
                    {
                        systemPrompt += $"- {p.Name}: Giá {p.Price:N0}đ (Tồn kho: {p.StockQuantity})\n";
                    }
                }
            } 
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách sản phẩm từ DB.");
            }

            systemPrompt += "\nCâu hỏi của khách: ";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + userMessage }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString();

                    return text ?? "Xin lỗi, hiện tại AI không có phản hồi.";
                }
                else
                {
                    _logger.LogWarning("Gemini API Error: " + responseString + ". Chuyển sang chế độ Offline (Keyword matching).");
                    return GenerateFallbackReply(userMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gọi Gemini API. Chuyển sang chế độ Offline.");
                return GenerateFallbackReply(userMessage);
            }
        }

        private string GenerateFallbackReply(string message)
        {
            string msg = message.ToLower().Trim();
            
            if (msg.Contains("chào") || msg.Contains("hello") || msg.Contains("hi"))
                return "Chào bạn! Chào mừng bạn đến với TechZone. Mình có thể giúp gì cho bạn hôm nay?";
            if (msg.Contains("ship") || msg.Contains("vận chuyển") || msg.Contains("giao hàng"))
                return "TechZone **miễn phí giao hàng toàn quốc** cho mọi đơn hàng nhé! Thời gian giao từ 2-4 ngày làm việc ạ.";
            if (msg.Contains("bảo hành") || msg.Contains("lỗi") || msg.Contains("hư hỏng"))
                return "Tất cả sản phẩm tại TechZone đều được **bảo hành chính hãng từ 12-24 tháng**. Nếu có lỗi từ nhà sản xuất, bạn được đổi mới trong 30 ngày đầu tiên nhé!";
            if (msg.Contains("thanh toán") || msg.Contains("cod") || msg.Contains("chuyển khoản") || msg.Contains("momo"))
                return "TechZone hỗ trợ 3 phương thức thanh toán: \n- Thanh toán khi nhận hàng (COD)\n- Chuyển khoản ngân hàng (QR Code)\n- Thanh toán qua Ví Momo.";
            if (msg.Contains("trả góp"))
                return "Hiện tại TechZone đang hỗ trợ trả góp 0% qua thẻ tín dụng và các công ty tài chính. Bạn vui lòng liên hệ Hotline để được hướng dẫn chi tiết nhé.";
            if (msg.Contains("cửa hàng") || msg.Contains("địa chỉ") || msg.Contains("ở đâu"))
                return "TechZone hiện có mặt tại:\n📍 123 Đường Công Nghệ, Quận 1, TP.HCM.\nHoạt động từ 8:00 đến 21:00 các ngày trong tuần.";
            if (msg.Contains("cảm ơn") || msg.Contains("thank"))
                return "Dạ không có gì ạ! Chúc bạn một ngày tốt lành và mua sắm vui vẻ cùng TechZone nhé! ❤️";
            if (msg.Contains("giá") || msg.Contains("nhiêu"))
                return "Dạ giá của mỗi sản phẩm đã được niêm yết công khai trên website. Nếu bạn quan tâm một mã sản phẩm cụ thể nào, hãy dùng thanh tìm kiếm hoặc duyệt qua Danh mục nhé!";

            return "Dạ em là trợ lý ảo TechZone. Hệ thống AI thông minh hiện đang bảo trì, nên em chỉ có thể trả lời các thông tin cơ bản. Bạn có thể diễn đạt bằng các từ khóa ngắn gọn hơn (VD: phí ship, bảo hành), hoặc gọi Hotline 1900 1234 ạ!";
        }
    }
}
