using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    public class ChatController : Controller
    {
        private readonly IAiAssistantService _aiAssistantService;

        public ChatController(IAiAssistantService aiAssistantService)
        {
            _aiAssistantService = aiAssistantService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    success = false,
                    response = "Please enter a message."
                });
            }

            var response = await _aiAssistantService.GetResponseAsync(message);

            return Json(new
            {
                success = true,
                response = response
            });
        }
    }
}