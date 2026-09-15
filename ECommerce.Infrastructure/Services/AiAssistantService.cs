using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using OpenAI;

namespace ECommerce.Infrastructure.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatClient _chatClient;

        public AiAssistantService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;

            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key was not found. Please add OpenAI:ApiKey to User Secrets.");
            }

            _chatClient = new ChatClient(
    model: "openrouter/free",
    credential: new ApiKeyCredential(apiKey),
    options: new OpenAIClientOptions
    {
        Endpoint = new Uri("https://openrouter.ai/api/v1")
    });
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a message.";
            }

            string message = userMessage.Trim().ToLower();

            // =========================
            // Greeting
            // =========================

            if (message.Contains("hello") ||
                message.Contains("hi") ||
                message.Contains("hey") ||
                message.Contains("مرحبا") ||
                message.Contains("اهلا") ||
                message.Contains("أهلا") ||
                message.Contains("سلام"))
            {
                return "Hello! 👋 Welcome to our store. How can I help you?";
            }

            // =========================
            // Categories
            // =========================

            if (message.Contains("category") ||
                message.Contains("categories") ||
                message.Contains("قسم") ||
                message.Contains("اقسام") ||
                message.Contains("أقسام") ||
                message.Contains("عندكم ايه"))
            {
                var categories = await _context.Categories
                    .Select(c => c.Name)
                    .ToListAsync();

                if (!categories.Any())
                {
                    return "Sorry, there are no categories available.";
                }

                return "Our available categories are: " +
                       string.Join(", ", categories) +
                       ".";
            }

            // =========================
            // Help
            // =========================

            if (message.Contains("help") ||
                message.Contains("مساعدة") ||
                message.Contains("اساعد"))
            {
                return "I can help you find products, check prices, check availability, show product categories, or answer general questions.";
            }

            // =========================
            // Search products
            // =========================

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    message.Contains(p.Name.ToLower()) ||
                    message.Contains(p.Category.Name.ToLower()))
                .Take(5)
                .ToListAsync();

            if (products.Any())
            {
                var response = "I found these products for you:\n\n";

                foreach (var product in products)
                {
                    response +=
                        $"• {product.Name}\n" +
                        $"  Price: {product.Price:C}\n" +
                        $"  Available: {product.AvailableQuantity} items\n\n";
                }

                return response;
            }

            // =========================
            // OpenAI - General Questions
            // =========================

            try
            {
                List<ChatMessage> messages =
                [
                    new SystemChatMessage(
                    """
                    You are a helpful AI assistant for an e-commerce website called Marketly.

                    You can answer general questions about programming, technology,
                    education, science, mathematics, everyday topics, and other general subjects.

                    Answer in the same language used by the user.
                    If the user writes Arabic, answer in Arabic.
                    If the user writes English, answer in English.

                    Be helpful, clear, and concise.

                    Do not invent information about the store, products,
                    prices, categories, or stock availability.
                    """
                ),

                new UserChatMessage(userMessage)
                ];

                ChatCompletion completion =
                    await _chatClient.CompleteChatAsync(messages);

                if (completion.Content.Count == 0)
                {
                    return "Sorry, I couldn't generate a response.";
                }

                return string.Join(
       "\n",
       completion.Content.Select(x => x.Text)
   );
            }
            catch (Exception ex)
            {
                return $"OpenAI Error: {ex.Message}";
            }
        }
    }

}