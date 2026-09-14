using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [Authorize] // يجب أن يكون مسجلاً للدخول لاستخدام القائمة
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IWishlistRepository wishlistRepository, UserManager<ApplicationUser> userManager)
        {
            _wishlistRepository = wishlistRepository;
            _userManager = userManager;
        }

        // عرض منتجات قائمة الرغبات للعميل الحالي
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var wishlist = await _wishlistRepository.GetUserWishlistAsync(userId);
            return View(wishlist);
        }

        // إضافة منتج لقائمة الرغبات
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = _userManager.GetUserId(User);
            await _wishlistRepository.AddToWishlistAsync(userId, productId);
            return RedirectToAction("Index", "Home"); // العودة للرئيسية بعد الإضافة
        }

        // إزالة منتج من القائمة
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            await _wishlistRepository.RemoveFromWishlistAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
