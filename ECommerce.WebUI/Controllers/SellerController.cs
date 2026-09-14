using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public SellerController(
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }


        // =====================================================
        // SELLER DASHBOARD
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var userId =
                _userManager.GetUserId(User);


            var sellerProducts =
                await _productRepository
                    .GetProductsBySellerAsync(userId);


            var sellerOrders =
                await _orderRepository
                    .GetSellerOrdersAsync(userId);


            var sellerOrderItems =
                sellerOrders
                    .SelectMany(o => o.OrderItems)
                    .Where(
                        oi =>
                            oi.Product != null &&
                            oi.Product.SellerId == userId
                    );


            var totalRevenue =
                sellerOrders

                    .Where(
                        o =>
                            o.Status ==
                            OrderStatus.Delivered
                    )

                    .SelectMany(
                        o => o.OrderItems
                    )

                    .Where(
                        oi =>
                            oi.Product != null &&
                            oi.Product.SellerId == userId
                    )

                    .Sum(
                        oi =>
                            oi.UnitPrice *
                            oi.Quantity
                    );


            var model =
                new SellerDashboardViewModel
                {
                    TotalProducts =
                        sellerProducts.Count(),

                    TotalOrders =
                        sellerOrders.Count(),

                    TotalItemsSold =
                        sellerOrderItems.Sum(
                            oi => oi.Quantity
                        ),

                    TotalRevenue =
                        totalRevenue
                };


            return View(model);
        }


        // =====================================================
        // SELLER ORDERS
        // =====================================================

        public async Task<IActionResult> Orders()
        {
            var userId =
                _userManager.GetUserId(User);


            var orders =
                await _orderRepository
                    .GetSellerOrdersAsync(userId);


            return View(orders);
        }


        // =====================================================
        // UPDATE ORDER STATUS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            OrderStatus status)
        {
            var userId =
                _userManager.GetUserId(User);


            await _orderRepository
                .UpdateSellerOrderStatusAsync(
                    orderId,
                    userId,
                    status
                );


            TempData["Success"] =
                "Order status updated successfully.";


            return RedirectToAction(
                nameof(Orders)
            );
        }
    }
}