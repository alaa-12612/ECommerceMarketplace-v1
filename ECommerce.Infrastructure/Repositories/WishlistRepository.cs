using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(string userId)
        {
            return await _context.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.CustomerId == userId)
                .OrderByDescending(w => w.AddedOn)
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(string userId, int productId)
        {
            
            if (!await IsProductInWishlistAsync(userId, productId))
            {
                var item = new WishlistItem { CustomerId = userId, ProductId = productId };
                _context.WishlistItems.Add(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(int id)
        {
            var item = await _context.WishlistItems.FindAsync(id);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsProductInWishlistAsync(string userId, int productId)
        {
            return await _context.WishlistItems.AnyAsync(w => w.CustomerId == userId && w.ProductId == productId);
        }
    }
}
