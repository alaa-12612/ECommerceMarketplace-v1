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
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IProductService productService,
            ICategoryRepository categoryRepository,
            IReviewService reviewService,
            UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
            _reviewService = reviewService;
            _userManager = userManager;
        }


        // =====================================================
        // HOME
        // =====================================================

        public async Task<IActionResult> Index(
            string? searchQuery,
            int? categoryId,
            string? sortOrder)
        {
            // Admin goes to Admin Dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("Seller"))
                {
                    return RedirectToAction("Index", "Seller");
                }
            }

            var products = await _productService.GetCatalogProductsAsync(searchQuery, categoryId, sortOrder);

            var categories = await _categoryRepository.GetAllAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SortOrder = sortOrder;

            return View(products);
        }


        // =====================================================
        // PRODUCT DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            var reviews = await _reviewService.GetProductReviewsAsync(id);

            bool canReview = false;

            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Customer"))
            {
                var userId = _userManager.GetUserId(User);
                canReview = await _reviewService.CanUserReviewProductAsync(userId, id);
            }

            var viewModel = new ProductDetailsViewModel
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