using CMS.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AIChatService _aiChatService;

        public ChatController(AIChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public class ChatRequest
        {
            public string Message { get; set; }
        }

        public class ChatResponse
        {
            public string Reply { get; set; }
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Ok(new ChatResponse { Reply = "Bạn cần mình giúp gì ạ?" });
            }

            // Gọi AI Service để lấy câu trả lời
            string reply = await _aiChatService.AskGeminiAsync(request.Message);

            return Ok(new ChatResponse { Reply = reply });
        }
    }
}
