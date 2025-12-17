using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<List<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<List<ProductDto>> GetProductsForSellerAsync(string sellerId);
        Task<List<ProductDto>> GetProductsForSellerAsync(string sellerId, int pageNumber, int pageSize);
        Task<List<ProductDto>> GetProductsByIdsAsync(IEnumerable<string> ids);
        Task<ProductDto?> GetProductByIdAsync(string id);
        Task<List<ProductDto>> SearchProductsAsync(string sellerId, string searchTerm, int? minStock = null, int pageNumber = 1, int pageSize = 10);
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task<ImportResultDto> ImportProductsAsync(Stream fileStream, string sellerId);
        Task UpdateProductAsync(string id, UpdateProductDto productDto);
        Task DeleteProductAsync(string id);
    }
}
