using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);

        Task<bool> CanUserReviewProductAsync(string? userId, int productId);

        Task<(bool Success, string Message)> AddReviewAsync(string? userId, int productId, int rating, string comment);

        Task<double> GetAverageRatingAsync(int productId);
    }
}
