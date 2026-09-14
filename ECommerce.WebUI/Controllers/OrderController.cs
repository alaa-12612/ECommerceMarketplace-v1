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
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
        }


        // =====================================================
        // CHECKOUT
        // =====================================================

        public async Task<IActionResult> Checkout()
        {
            var userId =
                _userManager.GetUserId(User);


            var order =
                await _orderRepository
                    .CreateOrderAsync(userId);


            if (order == null)
            {
                TempData["Error"] =
                    "Your cart is empty or there is not enough stock.";

                return RedirectToAction(
                    "Index",
                    "Cart"
                );
            }


            return View(
                "CheckoutComplete",
                order.Id
            );
        }


        // =====================================================
        // MY ORDERS
        // =====================================================

        public async Task<IActionResult> MyOrders()
        {
            var userId =
                _userManager.GetUserId(User);


            var orders =
                await _orderRepository
                    .GetCustomerOrdersAsync(userId);


            return View(orders);
        }


        // =====================================================
        // ORDER DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int id)
        {
            var userId =
                _userManager.GetUserId(User);


            var order =
                await _orderRepository
                    .GetOrderByIdAsync(id);


            if (order == null)
                return NotFound();


            // Customer can see only
            // their own order

            if (order.CustomerId != userId)
                return Forbid();


            return View(order);
        }


        // =====================================================
        // CANCEL ORDER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId =
                _userManager.GetUserId(User);


            var result =
                await _orderRepository
                    .CancelOrderAsync(
                        id,
                        userId
                    );


            if (!result)
            {
                TempData["Error"] =
                    "This order cannot be cancelled.";

                return RedirectToAction(
                    nameof(MyOrders)
                );
            }


            TempData["Success"] =
                "Order cancelled successfully.";


            return RedirectToAction(
                nameof(MyOrders)
            );
        }
    }
}