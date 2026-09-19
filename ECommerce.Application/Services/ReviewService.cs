using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _reviewRepository.GetProductReviewsAsync(productId);
        }

        public async Task<bool> CanUserReviewProductAsync(string? userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            bool hasPurchased = await _reviewRepository.HasUserPurchasedProductAsync(userId, productId);
            bool hasReviewed = await _reviewRepository.HasUserReviewedProductAsync(userId, productId);

            return hasPurchased && !hasReviewed;
        }

        public async Task<(bool Success, string Message)> AddReviewAsync(
            string? userId,
            int productId,
            int rating,
            string comment)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "You must be logged in to leave a review.");

            if (rating < 1 || rating > 5)
                return (false, "Rating must be between 1 and 5.");

            if (string.IsNullOrWhiteSpace(comment))
                return (false, "Please write a comment.");

            bool hasPurchased = await _reviewRepository.HasUserPurchasedProductAsync(userId, productId);
            if (!hasPurchased)
                return (false, "You can review only products you purchased.");

            bool hasReviewed = await _reviewRepository.HasUserReviewedProductAsync(userId, productId);
            if (hasReviewed)
                return (false, "You have already reviewed this product.");

            var review = new Review
            {
                CustomerId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            await _reviewRepository.AddReviewAsync(review);
            return (true, "Review submitted successfully.");
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
            return reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
        }
    }
}
