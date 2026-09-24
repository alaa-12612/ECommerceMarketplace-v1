using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }
        //GetCart
        public async Task<Cart> GetCartAsync(string customerId)
        {
            return await _cartRepository.GetCartByCustomerIdAsync(customerId);
        }

        // AddToCart
        public async Task<(bool Success, string Message)> AddToCartAsync(
            string customerId,
            int productId,
            int quantity)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return (false, "User is not authenticated.");

            if (quantity <= 0)
                return (false, "Quantity must be greater than zero.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            if (product.AvailableQuantity <= 0)
                return (false, "This product is currently out of stock.");

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            var existingItem = cart.Items?.FirstOrDefault(i => i.ProductId == productId);
            int currentQuantity = existingItem?.Quantity ?? 0;

            if (currentQuantity + quantity > product.AvailableQuantity)
            {
                return (false, $"Cannot add more. Maximum available stock is {product.AvailableQuantity}.");
            }

            await _cartRepository.AddItemToCartAsync(customerId, productId, quantity);
            return (true, "Product added to cart successfully.");
        }

        //UpdateQuantity
        public async Task<(bool Success, string Message)> UpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                await _cartRepository.RemoveItemFromCartAsync(cartItemId);
                return (true, "Item removed from cart.");
            }

            await _cartRepository.UpdateItemQuantityAsync(cartItemId, newQuantity);
            return (true, "Quantity updated successfully.");
        }

        //RemoveFromCart
        public async Task RemoveFromCartAsync(int cartItemId)
        {
            await _cartRepository.RemoveItemFromCartAsync(cartItemId);
        }

        //ClearCart
        public async Task ClearCartAsync(string customerId)
        {
            await _cartRepository.ClearCartAsync(customerId);
        }

        //GetCartTotal
        public async Task<decimal> GetCartTotalAsync(string customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart?.Items == null || !cart.Items.Any())
                return 0m;

            return cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0m));
        }
    }
}
