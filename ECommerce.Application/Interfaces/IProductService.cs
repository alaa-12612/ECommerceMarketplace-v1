using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetCatalogProductsAsync(string? searchQuery, int? categoryId, string? sortOrder);

        Task<IEnumerable<Product>> GetSellerProductsAsync(string sellerId, string? searchQuery = null, int? categoryId = null, string? sortOrder = null);

        Task<Product?> GetProductByIdAsync(int id);

        Task AddProductAsync(Product product);

        Task UpdateProductAsync(Product product);

        Task DeleteProductAsync(int id);
    }
}
