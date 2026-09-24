using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(string userId);

        Task<(bool Success, string Message)> AddToWishlistAsync(string userId, int productId);

        Task RemoveFromWishlistAsync(int id);

        Task<bool> IsProductInWishlistAsync(string userId, int productId);
    }
}
