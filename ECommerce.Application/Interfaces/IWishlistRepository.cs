using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(string userId);
        Task AddToWishlistAsync(string userId, int productId);
        Task RemoveFromWishlistAsync(int id);
        Task<bool> IsProductInWishlistAsync(string userId, int productId);
    }
}
