using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.WebUI.Models;
using ECommerce.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _userManager = userManager;
        }


        // =====================================================
        // HOME
        // =====================================================

        public async Task<IActionResult> Index(
            string searchQuery,
            int? categoryId,
            string sortOrder)
        {
            // Admin goes to Admin Dashboard

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Admin"
                    );
                }


                if (User.IsInRole("Seller"))
                {
                    return RedirectToAction(
                        "Index",
                        "Seller"
                    );
                }
            }


            var products =
                await _productRepository.GetAllAsync();


            // Search

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                products = products.Where(
                    p =>
                        p.Name.Contains(
                            searchQuery,
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        p.Description.Contains(
                            searchQuery,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
            }


            // Category filter

            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryId == categoryId.Value
                );
            }


            // Sort by price

            products = sortOrder switch
            {
                "price_asc" =>
                    products.OrderBy(p => p.Price),

                "price_desc" =>
                    products.OrderByDescending(p => p.Price),

                _ =>
                    products
            };


            var categories =
                await _categoryRepository.GetAllAsync();


            ViewBag.Categories =
                new SelectList(
                    categories,
                    "Id",
                    "Name"
                );


            ViewBag.SortOrder = sortOrder;


            return View(products);
        }


        // =====================================================
        // PRODUCT DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int id)
        {
            var product =
                await _productRepository.GetByIdAsync(id);


            if (product == null)
                return NotFound();


            var reviews =
                await _reviewRepository
                    .GetProductReviewsAsync(id);


            bool canReview = false;


            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Customer"))
            {
                var userId =
                    _userManager.GetUserId(User);


                bool hasPurchased =
                    await _reviewRepository
                        .HasUserPurchasedProductAsync(
                            userId,
                            id
                        );


                bool hasReviewed =
                    await _reviewRepository
                        .HasUserReviewedProductAsync(
                            userId,
                            id
                        );


                canReview =
                    hasPurchased &&
                    !hasReviewed;
            }


            var viewModel =
                new ProductDetailsViewModel
                {
                    Product = product,

                    Reviews = reviews,

                    CanReview = canReview
                };


            return View(viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}