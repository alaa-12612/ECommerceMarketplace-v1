using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, Order? Order, string Message)> CheckoutAsync(string customerId);

        Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId);

        Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null, bool isPrivileged = false);

        Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string customerId);

        Task<IEnumerable<Order>> GetAllOrdersAsync();

        Task<IEnumerable<Order>> GetSellerOrdersAsync(string sellerId);

        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);

        Task<bool> UpdateSellerOrderStatusAsync(int orderId, string sellerId, OrderStatus status);
    }
}
