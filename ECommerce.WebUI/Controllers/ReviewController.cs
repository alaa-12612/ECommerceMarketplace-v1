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
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(
            IReviewService reviewService,
            UserManager<ApplicationUser> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(
            int productId,
            int rating,
            string comment)
        {
            var userId = _userManager.GetUserId(User);

            
            var result = await _reviewService.AddReviewAsync(userId, productId, rating, comment);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            return RedirectToAction(
                "Details",
                "Home",
                new { id = productId }
            );
        }
    }
}