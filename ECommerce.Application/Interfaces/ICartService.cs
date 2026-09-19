using ECommerce.Domain.Entities;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(string customerId);

        Task<(bool Success, string Message)> AddToCartAsync(string customerId, int productId, int quantity);

        Task<(bool Success, string Message)> UpdateQuantityAsync(int cartItemId, int newQuantity);

        Task RemoveFromCartAsync(int cartItemId);

        Task ClearCartAsync(string customerId);

        Task<decimal> GetCartTotalAsync(string customerId);
    }
}
