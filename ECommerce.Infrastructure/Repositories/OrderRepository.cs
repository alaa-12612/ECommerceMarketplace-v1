using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // CREATE ORDER
        // =====================================================

        public async Task<Order> CreateOrderAsync(string customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null || !cart.Items.Any())
                return null;


            // Check stock before creating the order

            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                    return null;

                if (item.Quantity <= 0)
                    return null;

                if (item.Quantity > item.Product.AvailableQuantity)
                    return null;
            }


            var order = new Order
            {
                CustomerId = customerId,

                OrderDate = DateTime.Now,

                Status = OrderStatus.Pending,

                TotalPrice = cart.Items.Sum(
                    i => i.Quantity * i.Product.Price
                ),

                OrderItems = cart.Items.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,

                    Quantity = ci.Quantity,

                    UnitPrice = ci.Product.Price

                }).ToList()
            };


            // =================================================
            // DECREASE STOCK
            // =================================================

            foreach (var item in cart.Items)
            {
                item.Product.AvailableQuantity -= item.Quantity;
            }


            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return order;
        }


        // =====================================================
        // CUSTOMER ORDERS
        // =====================================================

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(
            string customerId)
        {
            return await _context.Orders

                .Include(o => o.Customer)

                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)

                .Where(o => o.CustomerId == customerId)

                .OrderByDescending(o => o.OrderDate)

                .ToListAsync();
        }


        // =====================================================
        // ALL ORDERS
        // =====================================================

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders

                .Include(o => o.Customer)

                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)

                .OrderByDescending(o => o.OrderDate)

                .ToListAsync();
        }


        // =====================================================
        // ORDER DETAILS
        // =====================================================

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders

                .Include(o => o.Customer)

                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)

                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        // =====================================================
        // ADMIN UPDATE STATUS
        // =====================================================

        public async Task UpdateOrderStatusAsync(
            int orderId,
            OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);


            if (order == null)
                return;


            // =================================================
            // IF ORDER IS BEING CANCELLED
            // RETURN STOCK
            // =================================================

            if (status == OrderStatus.Cancelled &&
                order.Status != OrderStatus.Cancelled)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    // Get the product directly from database
                    var product = await _context.Products
                        .FirstOrDefaultAsync(
                            p => p.Id == orderItem.ProductId
                        );

                    if (product != null)
                    {
                        product.AvailableQuantity += orderItem.Quantity;
                    }
                }
            }


            // Update order status

            order.Status = status;


            await _context.SaveChangesAsync();
        }


        // =====================================================
        // CUSTOMER CANCEL ORDER
        // =====================================================

        public async Task<bool> CancelOrderAsync(
            int orderId,
            string customerId)
        {
            var order = await _context.Orders

                .Include(o => o.OrderItems)

                .FirstOrDefaultAsync(
                    o => o.Id == orderId &&
                         o.CustomerId == customerId
                );


            if (order == null)
                return false;


            // Customer can cancel only
            // Pending or Confirmed orders

            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
            {
                return false;
            }


            // =================================================
            // RETURN PRODUCTS TO STOCK
            // =================================================

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == orderItem.ProductId
                    );

                if (product != null)
                {
                    product.AvailableQuantity += orderItem.Quantity;
                }
            }


            order.Status = OrderStatus.Cancelled;


            await _context.SaveChangesAsync();

            return true;
        }


        // =====================================================
        // SELLER ORDERS
        // =====================================================

        public async Task<IEnumerable<Order>> GetSellerOrdersAsync(
            string sellerId)
        {
            return await _context.Orders

                .Include(o => o.Customer)

                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)

                .Where(o =>
                    o.OrderItems.Any(
                        i => i.Product != null &&
                             i.Product.SellerId == sellerId
                    )
                )

                .OrderByDescending(o => o.OrderDate)

                .ToListAsync();
        }


        // =====================================================
        // SELLER UPDATE ORDER STATUS
        // =====================================================
     public async Task<bool> UpdateSellerOrderStatusAsync(
            int orderId,
            string sellerId,
            OrderStatus status)
        {
            var order = await _context.Orders

                .Include(o => o.OrderItems)

                .FirstOrDefaultAsync(
                    o => o.Id == orderId
                );


            if (order == null)
                return false;


            // Make sure this order contains
            // at least one product belonging to this seller

            var belongsToSeller = false;

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == item.ProductId
                    );

                if (product != null &&
                    product.SellerId == sellerId)
                {
                    belongsToSeller = true;
                    break;
                }
            }


            if (!belongsToSeller)
                return false;


            // =================================================
            // IF SELLER CANCELS THE ORDER
            // RETURN STOCK
            // =================================================

            if (status == OrderStatus.Cancelled &&
                order.Status != OrderStatus.Cancelled)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(
                            p => p.Id == orderItem.ProductId
                        );

                    if (product != null)
                    {
                        product.AvailableQuantity += orderItem.Quantity;
                    }
                }
            }


            order.Status = status;


            await _context.SaveChangesAsync();

            return true;
        }
    }
}




