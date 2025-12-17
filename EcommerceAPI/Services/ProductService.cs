using System;
using System.Linq;
using EcommerceAPI.Configuration;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IFirestoreService _firestoreService;
        private readonly IExcelService _excelService;
        private readonly SellerSettings _sellerSettings;
        private const string CollectionName = "Products";

        public ProductService(
            IFirestoreService firestoreService,
            IExcelService excelService,
            IOptions<SellerSettings> sellerOptions)
        {
            _firestoreService = firestoreService;
            _excelService = excelService;
            _sellerSettings = sellerOptions.Value;
        }

        public async Task<ImportResultDto> ImportProductsAsync(Stream fileStream, string sellerId)
        {
            var normalizedSellerId = ResolveSellerId(sellerId);
            var products = await _excelService.ImportProductsFromExcel(fileStream, normalizedSellerId);
            var result = new ImportResultDto();

            if (products.Count == 0)
            {
                result.Message = "Excel dosyasında ürün bulunamadı veya format hatalı.";
                return result;
            }

            foreach (var product in products)
            {
                try
                {
                    // Doğrudan Firestore'a ekleyebiliriz veya CreateProductAsync kullanabiliriz.
                    // CreateProductDto dönüşümü yaparak mevcut mantığı kullanalım.
                    var createDto = new CreateProductDto
                    {
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Stock = product.Stock,
                        Category = product.Category,
                        SKU = product.SKU,
                        ImageUrl = product.ImageUrl,
                        SellerId = ResolveSellerId(product.SellerId)
                    };
                    
                    await CreateProductAsync(createDto);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Ürün: {product.Name} - Hata: {ex.Message}");
                }
            }

            result.Message = $"{result.SuccessCount} ürün başarıyla içe aktarıldı. {(result.FailureCount > 0 ? $"{result.FailureCount} ürün yüklenemedi." : "")}";
            return result;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _firestoreService.GetAllDocumentsAsync<Product>(CollectionName);
            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            var products = await _firestoreService.GetDocumentsPagedAsync<Product>(CollectionName, pageNumber, pageSize, nameof(Product.CreatedAt), true);
            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductDto>> GetProductsForSellerAsync(string sellerId)
        {
            var products = await GetProductsForSellerInternalAsync(sellerId);
            return products
                .OrderByDescending(p => p.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<List<ProductDto>> GetProductsForSellerAsync(string sellerId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var products = await GetProductsForSellerInternalAsync(sellerId);
            var ordered = products.OrderByDescending(p => p.CreatedAt);
            var paged = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return paged.Select(MapToDto).ToList();
        }

        private async Task<List<Product>> GetProductsForSellerInternalAsync(string sellerId)
        {
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                throw new ArgumentException("SellerId cannot be empty", nameof(sellerId));
            }

            if (_sellerSettings.UseSharedSeller)
            {
                return await _firestoreService.GetAllDocumentsAsync<Product>(CollectionName);
            }

            var effectiveSellerId = string.IsNullOrWhiteSpace(sellerId)
                ? _sellerSettings.SharedSellerId
                : sellerId;

            var products = await _firestoreService.QueryDocumentsAsync<Product>(
                CollectionName,
                nameof(Product.SellerId),
                effectiveSellerId);

            return products;
        }

        public async Task<List<ProductDto>> GetProductsByIdsAsync(IEnumerable<string> ids)
        {
            var products = await _firestoreService.GetDocumentsByIdsAsync<Product>(CollectionName, ids);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            var product = await _firestoreService.GetDocumentAsync<Product>(CollectionName, id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                Category = productDto.Category,
                SKU = productDto.SKU,
                ImageUrl = productDto.ImageUrl,
                SellerId = ResolveSellerId(productDto.SellerId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var id = await _firestoreService.AddDocumentAsync(CollectionName, product);
            product.Id = id;
            return MapToDto(product);
        }

        private string ResolveSellerId(string? requestedSellerId)
        {
            if (_sellerSettings.UseSharedSeller && !string.IsNullOrWhiteSpace(_sellerSettings.SharedSellerId))
            {
                return _sellerSettings.SharedSellerId;
            }

            return requestedSellerId ?? string.Empty;
        }

        public async Task UpdateProductAsync(string id, UpdateProductDto productDto)
        {
            var updates = new Dictionary<string, object>();

            if (productDto.Name != null) updates.Add(nameof(Product.Name), productDto.Name);
            if (productDto.Description != null) updates.Add(nameof(Product.Description), productDto.Description);
            if (productDto.Price.HasValue) updates.Add(nameof(Product.Price), productDto.Price.Value);
            if (productDto.Stock.HasValue) updates.Add(nameof(Product.Stock), productDto.Stock.Value);
            if (productDto.Category != null) updates.Add(nameof(Product.Category), productDto.Category);
            if (productDto.SKU != null) updates.Add(nameof(Product.SKU), productDto.SKU);
            if (productDto.ImageUrl != null) updates.Add(nameof(Product.ImageUrl), productDto.ImageUrl);
            if (productDto.IsActive.HasValue) updates.Add(nameof(Product.IsActive), productDto.IsActive.Value);
            
            updates.Add(nameof(Product.UpdatedAt), DateTime.UtcNow);

            await _firestoreService.UpdateDocumentAsync(CollectionName, id, updates);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _firestoreService.DeleteDocumentAsync(CollectionName, id);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                SKU = product.SKU,
                ImageUrl = product.ImageUrl,
                SellerId = product.SellerId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive
            };
        }
    }
}
