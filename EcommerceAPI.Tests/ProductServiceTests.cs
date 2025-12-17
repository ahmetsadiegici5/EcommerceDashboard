using Xunit;
using Moq;
using EcommerceAPI.Services;
using EcommerceAPI.Models;
using EcommerceAPI.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using EcommerceAPI.Configuration;

namespace EcommerceAPI.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IFirestoreService> _mockFirestoreService;
        private readonly Mock<IExcelService> _mockExcelService;
        private readonly Mock<IOptions<SellerSettings>> _mockSellerSettings;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockFirestoreService = new Mock<IFirestoreService>();
            _mockExcelService = new Mock<IExcelService>();
            _mockSellerSettings = new Mock<IOptions<SellerSettings>>();

            // Varsayılan ayarları döndür
            _mockSellerSettings.Setup(s => s.Value).Returns(new SellerSettings 
            { 
                UseSharedSeller = false,
                SharedSellerId = "shared-seller" 
            });

            _productService = new ProductService(
                _mockFirestoreService.Object, 
                _mockExcelService.Object,
                _mockSellerSettings.Object);
        }

        [Fact]
        public async Task ImportProductsAsync_Should_Import_Products_Successfully()
        {
            // Arrange
            var sellerId = "test-seller-id";
            var products = new List<Product>
            {
                new Product { Name = "Product 1", Price = 100, Stock = 10, SellerId = sellerId },
                new Product { Name = "Product 2", Price = 200, Stock = 20, SellerId = sellerId }
            };

            _mockExcelService.Setup(x => x.ImportProductsFromExcel(It.IsAny<Stream>(), sellerId))
                .ReturnsAsync(products);

            _mockFirestoreService.Setup(x => x.AddDocumentAsync(It.IsAny<string>(), It.IsAny<Product>()))
                .ReturnsAsync("new-doc-id");

            using var stream = new MemoryStream();

            // Act
            var result = await _productService.ImportProductsAsync(stream, sellerId);

            // Assert
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Empty(result.Errors);
            
            // Verify that AddDocumentAsync was called 2 times (once for each product)
            // Note: ProductService calls CreateProductAsync internally, which calls AddDocumentAsync
            _mockFirestoreService.Verify(x => x.AddDocumentAsync("Products", It.IsAny<Product>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ImportProductsAsync_Should_Handle_Errors_Gracefully()
        {
            // Arrange
            var sellerId = "test-seller-id";
            var products = new List<Product>
            {
                new Product { Name = "Valid Product", Price = 100 },
                new Product { Name = "Invalid Product", Price = 200 }
            };

            _mockExcelService.Setup(x => x.ImportProductsFromExcel(It.IsAny<Stream>(), sellerId))
                .ReturnsAsync(products);

            // First call succeeds
            _mockFirestoreService.SetupSequence(x => x.AddDocumentAsync(It.IsAny<string>(), It.IsAny<Product>()))
                .ReturnsAsync("id-1")
                .ThrowsAsync(new System.Exception("Database error")); // Second call fails

            using var stream = new MemoryStream();

            // Act
            var result = await _productService.ImportProductsAsync(stream, sellerId);

            // Assert
            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            Assert.Contains("Database error", result.Errors[0]);
        }
    }
}
