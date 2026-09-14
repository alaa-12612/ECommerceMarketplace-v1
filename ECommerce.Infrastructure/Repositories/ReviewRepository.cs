using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Review>>
            GetProductReviewsAsync(int productId)
        {
            return await _context.Reviews

                .Include(r => r.Customer)

                .Where(r => r.ProductId == productId)

                .OrderByDescending(r => r.CreatedAt)

                .ToListAsync();
        }


        public async Task AddReviewAsync(
            Review review)
        {
            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();
        }


        public async Task<bool>
            HasUserReviewedProductAsync(
                string userId,
                int productId)
        {
            return await _context.Reviews.AnyAsync(
                r =>
                    r.CustomerId == userId &&
                    r.ProductId == productId
            );
        }


        // Customer must have purchased
        // the product before reviewing it

        public async Task<bool>
            HasUserPurchasedProductAsync(
                string userId,
                int productId)
        {
            return await _context.Orders

                .AnyAsync(
                    o =>
                        o.CustomerId == userId &&

                        o.Status != OrderStatus.Cancelled &&

                        o.OrderItems.Any(
                            oi =>
                                oi.ProductId == productId
                        )
                );
        }
    }
}