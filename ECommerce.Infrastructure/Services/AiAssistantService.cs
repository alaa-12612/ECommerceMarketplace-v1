using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ApplicationDbContext _context;

        public AiAssistantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a message.";
            }

            string message = userMessage.Trim().ToLower();

            // Greeting
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

            // Categories
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

            // Help
            if (message.Contains("help") ||
                message.Contains("مساعدة") ||
                message.Contains("اساعد"))
            {
                return "I can help you find products, check prices, check availability, and show product categories.";
            }

            // Search products
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

            // Default response
            return "Sorry, I couldn't find what you are looking for. " +
                   "Try asking about a product, category, price, or availability.";
        }
    }
}