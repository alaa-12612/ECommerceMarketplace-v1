using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderRepository
    {
   
        Task<Order> CreateOrderAsync(string customerId);

        Task<IEnumerable<Order>> GetCustomerOrdersAsync(string customerId);

        Task<IEnumerable<Order>> GetAllOrdersAsync();

        Task<Order> GetOrderByIdAsync(int orderId);

        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

        Task<bool> CancelOrderAsync(int orderId, string customerId);

        Task<IEnumerable<Order>> GetSellerOrdersAsync(string sellerId);

        Task<bool> UpdateSellerOrderStatusAsync(
            int orderId,
            string sellerId,
            OrderStatus status);
    }
}