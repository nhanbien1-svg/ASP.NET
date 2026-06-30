using System.Text;
using System.Text.Json;

namespace CMS.Backend.Services
{
    public class AIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(HttpClient httpClient, IConfiguration configuration, ILogger<AIChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> AskGeminiAsync(string userMessage)
        {
            string apiKey = _configuration["GeminiApi:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Hệ thống AI chưa được cấu hình API Key. Vui lòng cập nhật appsettings.json.";
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            // Định nghĩa prompt bối cảnh cho Chatbot
            string systemPrompt = @"Bạn là trợ lý ảo tư vấn khách hàng của cửa hàng điện tử TechZone (chuyên bán Laptop, Điện thoại).
Nhiệm vụ của bạn:
- Luôn thân thiện, lịch sự và xưng hô là 'mình' - 'bạn' hoặc 'dạ em' - 'anh/chị'.
- Trả lời ngắn gọn, đúng trọng tâm câu hỏi, không dài dòng.
- Nếu không biết, hãy nói xin lỗi và khuyên khách hàng gọi Hotline 1900 1234.
- Sử dụng tiếng Việt một cách tự nhiên.
Câu hỏi của khách: ";

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
