using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Configuration;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using EcommerceAPI.Models;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IExcelService _excelService;
        private readonly ICurrentUserService _currentUserService;
        private readonly SellerSettings _sellerSettings;

        public ProductsController(
            IProductService productService, 
            IExcelService excelService,
            ICurrentUserService currentUserService,
            IOptions<SellerSettings> sellerOptions)
        {
            _productService = productService;
            _excelService = excelService;
            _currentUserService = currentUserService;
            _sellerSettings = sellerOptions.Value;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (_sellerSettings.UseSharedSeller)
            {
                var sharedProducts = await _productService.GetAllProductsAsync(page, pageSize);
                return Ok(sharedProducts);
            }

            var sellerId = _currentUserService.GetUserIdOrThrow();
            var products = await _productService.GetProductsForSellerAsync(sellerId, page, pageSize);
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ProductDto>>> Search(
            [FromQuery] string q = "", 
            [FromQuery] int? lowStock = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var sellerId = _currentUserService.GetUserIdOrThrow();
            var products = await _productService.SearchProductsAsync(sellerId, q, lowStock, page, pageSize);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
                return NotFound($"ID: {id} olan ürün bulunamadı");

            if (!UserOwnsProduct(product))
                return Forbid();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto productDto)
        {
            // Güvenlik: SellerId'yi token'dan al
            productDto.SellerId = _currentUserService.GetUserIdOrThrow();

            var createdProduct = await _productService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UpdateProductDto productDto)
        {
            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan ürün bulunamadı");

            if (!UserOwnsProduct(existing))
                return Forbid();

            await _productService.UpdateProductAsync(id, productDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan ürün bulunamadı");

            if (!UserOwnsProduct(existing))
                return Forbid();

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        // Excel İşlemleri
        [HttpPost("import")]
        public async Task<ActionResult> ImportFromExcel(IFormFile file)
        {
            var sellerId = _currentUserService.GetUserIdOrThrow();

            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                return BadRequest("Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir");

            using var stream = file.OpenReadStream();
            var result = await _productService.ImportProductsAsync(stream, sellerId);

            if (result.SuccessCount == 0 && result.FailureCount > 0)
                return StatusCode(500, new { message = result.Message, errors = result.Errors });

            return Ok(new 
            { 
                message = result.Message, 
                count = result.SuccessCount,
                errors = result.Errors
            });
        }

        [HttpGet("export")]
        public async Task<ActionResult> ExportToExcel()
        {
            var sellerId = _currentUserService.GetUserIdOrThrow(); 
            
            var productDtos = await _productService.GetProductsForSellerAsync(sellerId);
            
            // Map DTOs back to Models for ExcelService
            var products = productDtos.Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                SellerId = p.SellerId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive
            }).ToList();

            var excelBytes = await _excelService.ExportProductsToExcel(products);
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"products_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet("template")]
        public async Task<ActionResult> DownloadTemplate()
        {
            var excelBytes = await _excelService.GenerateProductTemplate();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                "product_template.xlsx");
        }

        private bool UserOwnsProduct(ProductDto product)
        {
            if (_sellerSettings.UseSharedSeller)
            {
                return true;
            }

            var currentUserId = _currentUserService.GetUserIdOrThrow();
            return string.Equals(product.SellerId, currentUserId, StringComparison.Ordinal);
        }
    }
}
