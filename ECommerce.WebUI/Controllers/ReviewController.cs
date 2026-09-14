using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(
            IReviewRepository reviewRepository,
            UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _userManager = userManager;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(
            int productId,
            int rating,
            string comment)
        {
            var userId =
                _userManager.GetUserId(User);


            if (rating < 1 || rating > 5)
            {
                TempData["Error"] =
                    "Rating must be between 1 and 5.";

                return RedirectToAction(
                    "Details",
                    "Home",
                    new { id = productId }
                );
            }


            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] =
                    "Please write a comment.";

                return RedirectToAction(
                    "Details",
                    "Home",
                    new { id = productId }
                );
            }


            // Check if customer purchased the product

            bool hasPurchased =
                await _reviewRepository
                    .HasUserPurchasedProductAsync(
                        userId,
                        productId
                    );


            if (!hasPurchased)
            {
                TempData["Error"] =
                    "You can review only products you purchased.";

                return RedirectToAction(
                    "Details",
                    "Home",
                    new { id = productId }
                );
            }


            // Prevent duplicate reviews

            bool hasReviewed =
                await _reviewRepository
                    .HasUserReviewedProductAsync(
                        userId,
                        productId
                    );


            if (hasReviewed)
            {
                TempData["Error"] =
                    "You have already reviewed this product.";

                return RedirectToAction(
                    "Details",
                    "Home",
                    new { id = productId }
                );
            }


            var review = new Review
            {
                ProductId = productId,

                CustomerId = userId,

                Rating = rating,

                Comment = comment,

                CreatedAt = DateTime.Now
            };


            await _reviewRepository
                .AddReviewAsync(review);


            TempData["Success"] =
                "Your review was added successfully.";


            return RedirectToAction(
                "Details",
                "Home",
                new { id = productId }
            );
        }
    }
}