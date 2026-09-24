using ECommerce.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;

        public IEnumerable<Review> Reviews { get; set; } = Enumerable.Empty<Review>();

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write a comment.")]
        public string Comment { get; set; } = string.Empty;

        // Indicates whether the current logged-in user is eligible to review this product
        public bool CanReview { get; set; }

        // Overall average rating calculation 
        public double OverallRating => Reviews.Any() ? Math.Round(Reviews.Average(r => r.Rating), 1) : 0;

        // Total number of customer reviews
        public int TotalReviews => Reviews.Count();
    }
}