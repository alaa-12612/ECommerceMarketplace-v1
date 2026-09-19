using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // =====================================================
        // CHECKOUT
        // =====================================================
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);

            // استدعاء الـ Service الخاصة بالطلبات لإتمام عملية الشراء
            var result = await _orderService.CheckoutAsync(userId);

            if (!result.Success || result.Order == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Cart");
            }

            TempData["Success"] = result.Message;
            return View("CheckoutComplete", result.Order.Id);
        }

        // =====================================================
        // MY ORDERS
        // =====================================================
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _orderService.GetCustomerOrdersAsync(userId);

            return View(orders);
        }

        // =====================================================
        // ORDER DETAILS
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            // استخدام الـ Service للتحقق من الصلاحيات وجلب الطلب بأمان
            var order = await _orderService.GetOrderByIdAsync(id, userId, isPrivileged: false);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // =====================================================
        // CANCEL ORDER
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);

            var result = await _orderService.CancelOrderAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(MyOrders));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(MyOrders));
        }
    }
}