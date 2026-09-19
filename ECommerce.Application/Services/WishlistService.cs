using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<WishlistItem>();

            return await _wishlistRepository.GetUserWishlistAsync(userId);
        }

        public async Task<(bool Success, string Message)> AddToWishlistAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "User is not authenticated.");

            bool alreadyInWishlist = await _wishlistRepository.IsProductInWishlistAsync(userId, productId);
            if (alreadyInWishlist)
            {
                return (false, "Product is already in your wishlist.");
            }

            await _wishlistRepository.AddToWishlistAsync(userId, productId);
            return (true, "Product added to wishlist successfully.");
        }

        public async Task RemoveFromWishlistAsync(int id)
        {
            await _wishlistRepository.RemoveFromWishlistAsync(id);
        }

        public async Task<bool> IsProductInWishlistAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return await _wishlistRepository.IsProductInWishlistAsync(userId, productId);
        }
    }
}
