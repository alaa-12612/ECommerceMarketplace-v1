using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository : ECommerce.Application.Interfaces.ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET CART
        // =====================================================

        public async Task<Cart> GetCartByCustomerIdAsync(
            string customerId)
        {
            var cart = await _context.Carts

                .Include(c => c.Items)

                .ThenInclude(i => i.Product)

                .FirstOrDefaultAsync(
                    c => c.CustomerId == customerId
                );


            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }


            return cart;
        }


        // =====================================================
        // ADD TO CART
        // =====================================================

        public async Task AddItemToCartAsync(
            string customerId,
            int productId,
            int quantity)
        {
            if (quantity <= 0)
                return;


            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);


            if (product == null)
                return;


            if (product.AvailableQuantity <= 0)
                return;


            var cart = await GetCartByCustomerIdAsync(
                customerId
            );


            var existingItem = cart.Items
                .FirstOrDefault(
                    i => i.ProductId == productId
                );


            int currentQuantity =
                existingItem?.Quantity ?? 0;


            int newQuantity =
                currentQuantity + quantity;


            // Prevent exceeding stock

            if (newQuantity > product.AvailableQuantity)
                return;


            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                cart.Items.Add(
                    new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity
                    }
                );
            }


            await _context.SaveChangesAsync();
        }


        // =====================================================
        // UPDATE QUANTITY
        // =====================================================

        public async Task UpdateItemQuantityAsync(
            int cartItemId,
            int newQuantity)
        {
            var item = await _context.CartItems

                .Include(i => i.Product)

                .FirstOrDefaultAsync(
                    i => i.Id == cartItemId
                );


            if (item == null)
                return;


            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(item);

                await _context.SaveChangesAsync();

                return;
            }


            if (item.Product == null)
                return;


            // Prevent quantity greater than stock

            if (newQuantity > item.Product.AvailableQuantity)
                return;


            item.Quantity = newQuantity;


            await _context.SaveChangesAsync();
        }


        // =====================================================
        // REMOVE
        // =====================================================

        public async Task RemoveItemFromCartAsync(
            int cartItemId)
        {
            var item = await _context.CartItems
                .FindAsync(cartItemId);


            if (item == null)
                return;


            _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // CLEAR CART
        // =====================================================

        public async Task ClearCartAsync(
            string customerId)
        {
            var cart = await _context.Carts

                .Include(c => c.Items)

                .FirstOrDefaultAsync(
                    c => c.CustomerId == customerId
                );


            if (cart != null &&
                cart.Items.Any())
            {
                _context.CartItems.RemoveRange(
                    cart.Items
                );

                await _context.SaveChangesAsync();
            }
        }


        // =====================================================
        // GET CART BY USER ID
        // =====================================================

        public async Task<Cart> GetCartByUserIdAsync(
            string userId)
        {
            return await GetCartByCustomerIdAsync(
                userId
            );
        }
    }
}