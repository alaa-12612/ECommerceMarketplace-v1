using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(
             int productId);

        Task AddReviewAsync(
            Review review);

        Task<bool> HasUserReviewedProductAsync(
            string userId,
            int productId);

        Task<bool> HasUserPurchasedProductAsync(
            string userId,
            int productId);
    }
}
