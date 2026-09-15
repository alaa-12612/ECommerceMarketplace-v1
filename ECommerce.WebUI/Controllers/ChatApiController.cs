using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatApiController : ControllerBase
    {
        private readonly IAiAssistantService _aiAssistantService;

        public ChatApiController(IAiAssistantService aiAssistantService)
        {
            _aiAssistantService = aiAssistantService;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    response = "Please enter a message."
                });
            }

            var response =
                await _aiAssistantService.GetResponseAsync(request.Message);

            return Ok(new
            {
                success = true,
                response = response
            });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}