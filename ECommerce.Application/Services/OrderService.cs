using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<(bool Success, Order? Order, string Message)> CheckoutAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return (false, null, "User is not authenticated.");

            var order = await _orderRepository.CreateOrderAsync(customerId);
            if (order == null)
            {
                return (false, null, "Your cart is empty or there is not enough stock.");
            }

            return (true, order, "Order placed successfully.");
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return new List<Order>();

            return await _orderRepository.GetCustomerOrdersAsync(customerId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null, bool isPrivileged = false)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return null;

            if (!isPrivileged && !string.IsNullOrWhiteSpace(userId) && order.CustomerId != userId)
            {
                return null; // Not authorized to access this order
            }

            return order;
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return (false, "User is not authenticated.");

            bool cancelled = await _orderRepository.CancelOrderAsync(orderId, customerId);
            if (!cancelled)
            {
                return (false, "Unable to cancel order. It may have already been shipped or processed.");
            }

            return (true, "Order has been cancelled successfully.");
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<IEnumerable<Order>> GetSellerOrdersAsync(string sellerId)
        {
            if (string.IsNullOrWhiteSpace(sellerId))
                return new List<Order>();

            return await _orderRepository.GetSellerOrdersAsync(sellerId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
            return true;
        }

        public async Task<bool> UpdateSellerOrderStatusAsync(int orderId, string sellerId, OrderStatus status)
        {
            return await _orderRepository.UpdateSellerOrderStatusAsync(orderId, sellerId, status);
        }
    }
}
