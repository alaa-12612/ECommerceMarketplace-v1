using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ICartRepository cartRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var cart =
                await _cartRepository.GetCartByCustomerIdAsync(
                    userId
                );

            return View(cart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int productId,
            int quantity = 1)
        {
            if (quantity <= 0)
            {
                TempData["Error"] =
                    "Quantity must be greater than zero.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            var userId =
                _userManager.GetUserId(User);


            await _cartRepository.AddItemToCartAsync(
                userId,
                productId,
                quantity
            );


            TempData["Success"] =
                "Product added to cart successfully.";


            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(
            int cartItemId,
            int quantity)
        {
            if (quantity <= 0)
            {
                await _cartRepository
                    .RemoveItemFromCartAsync(cartItemId);

                return RedirectToAction(
                    nameof(Index)
                );
            }


            await _cartRepository
                .UpdateItemQuantityAsync(
                    cartItemId,
                    quantity
                );


            return RedirectToAction(
                nameof(Index)
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(
            int cartItemId)
        {
            await _cartRepository
                .RemoveItemFromCartAsync(
                    cartItemId
                );


            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}