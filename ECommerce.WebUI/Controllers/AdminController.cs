using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICartRepository cartRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
        }

        // لوحة التحكم الرئيسية للمدير
        public async Task<IActionResult> Index()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");
            var allOrders = await _orderRepository.GetAllOrdersAsync();
            var allProducts = await _productRepository.GetAllAsync();

            var model = new AdminDashboardViewModel
            {
                TotalCustomers = customers.Count,
                TotalSellers = sellers.Count,
                TotalProducts = allProducts.Count(),
                TotalOrders = allOrders.Count(),
                TotalRevenue = allOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                  PendingOrdersCount = allOrders.Count(o => o.Status == OrderStatus.Pending)
          
        };

            return View(model);
        }

   

        // --- إدارة طلبات البائعين ---
        public async Task<IActionResult> SellerRequests()
        {
            var requests = await _userManager.Users
                .Where(u => u.HasRequestedToBecomeSeller && !u.IsApprovedSeller)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ApproveSeller(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsApprovedSeller = true;
                user.HasRequestedToBecomeSeller = false;
                await _userManager.UpdateAsync(user);

                if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Customer");
                }

                await _userManager.AddToRoleAsync(user, "Seller");

                // ✅ إجبار Identity على إبطال الكوكي القديمة وتحديث صلاحيات المستخدم فوراً
                await _userManager.UpdateSecurityStampAsync(user);

                // تفريغ سلة التسوق
                await _cartRepository.ClearCartAsync(user.Id);
            }
            return RedirectToAction(nameof(SellerRequests));
        }

        [HttpPost]
        public async Task<IActionResult> RejectSeller(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.HasRequestedToBecomeSeller = false;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(SellerRequests));
        }

        // --- إدارة الطلبات ---
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            {
                await _orderRepository.UpdateOrderStatusAsync(orderId, parsedStatus);
            }
            return RedirectToAction(nameof(ManageOrders));
        }

        // عرض كافة المستخدمين
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // حظر / تعطيل حساب مستخدم
        [HttpPost]
        public async Task<IActionResult> SuspendUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // حظر الحساب لمدة 100 سنة على سبيل المثال
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            return RedirectToAction(nameof(Users));
        }

        // إلغاء الحظر / تفعيل حساب مستخدم
        [HttpPost]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // إزالة الحظر فوراً
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToAction(nameof(Users));
        }


        // --- إدارة كافة منتجات المتجر (Task 9) ---
        public async Task<IActionResult> Products()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // حذف / حجب منتج مخالف
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productRepository.DeleteAsync(id);
            TempData["Success"] = "Product has been removed successfully.";
            return RedirectToAction(nameof(Products));
        }

        // عرض تفاصيل طلب محدد
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(ManageOrders));
            }
            return View(order);
        }

    }
}