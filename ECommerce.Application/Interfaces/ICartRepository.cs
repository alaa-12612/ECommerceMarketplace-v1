using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerIdAsync(string customerId);
        Task<Cart> GetCartByUserIdAsync(string userId); // إضافة التوقيع هنا
        Task AddItemToCartAsync(string customerId, int productId, int quantity);
        Task UpdateItemQuantityAsync(int cartItemId, int newQuantity);
        Task RemoveItemFromCartAsync(int cartItemId);
        Task ClearCartAsync(string customerId);
    }
}
