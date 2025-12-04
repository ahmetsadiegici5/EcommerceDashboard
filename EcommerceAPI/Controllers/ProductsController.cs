using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly FirestoreService _firestoreService;
        private readonly ExcelService _excelService;
        private readonly ILogger<ProductsController> _logger;
        private const string COLLECTION_NAME = "products";

        public ProductsController(
            FirestoreService firestoreService, 
            ExcelService excelService,
            ILogger<ProductsController> logger)
        {
            _firestoreService = firestoreService;
            _excelService = excelService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll([FromQuery] string? sellerId = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(sellerId))
                {
                    var products = await _firestoreService.QueryDocumentsAsync<Product>(COLLECTION_NAME, "SellerId", sellerId);
                    return Ok(products);
                }

                var allProducts = await _firestoreService.GetAllDocumentsAsync<Product>(COLLECTION_NAME);
                return Ok(allProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünler getirilirken hata oluştu");
                return StatusCode(500, "Ürünler getirilirken bir hata oluştu");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(string id)
        {
            try
            {
                var product = await _firestoreService.GetDocumentAsync<Product>(COLLECTION_NAME, id);
                
                if (product == null)
                    return NotFound($"ID: {id} olan ürün bulunamadı");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün getirilirken hata oluştu");
                return StatusCode(500, "Ürün getirilirken bir hata oluştu");
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                
                var id = await _firestoreService.AddDocumentAsync(COLLECTION_NAME, product);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün eklenirken hata oluştu");
                return StatusCode(500, "Ürün eklenirken bir hata oluştu");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] Product product)
        {
            try
            {
                var existing = await _firestoreService.GetDocumentAsync<Product>(COLLECTION_NAME, id);
                if (existing == null)
                    return NotFound($"ID: {id} olan ürün bulunamadı");

                product.Id = id;
                product.UpdatedAt = DateTime.UtcNow;
                product.CreatedAt = existing.CreatedAt;
                
                await _firestoreService.UpdateDocumentAsync(COLLECTION_NAME, id, product);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata oluştu");
                return StatusCode(500, "Ürün güncellenirken bir hata oluştu");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var existing = await _firestoreService.GetDocumentAsync<Product>(COLLECTION_NAME, id);
                if (existing == null)
                    return NotFound($"ID: {id} olan ürün bulunamadı");

                await _firestoreService.DeleteDocumentAsync(COLLECTION_NAME, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün silinirken hata oluştu");
                return StatusCode(500, "Ürün silinirken bir hata oluştu");
            }
        }

        // Excel İşlemleri
        [HttpPost("import")]
        public async Task<ActionResult> ImportFromExcel(IFormFile file, [FromQuery] string sellerId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Dosya seçilmedi");

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                    return BadRequest("Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir");

                using var stream = file.OpenReadStream();
                var products = await _excelService.ImportProductsFromExcel(stream, sellerId);

                foreach (var product in products)
                {
                    await _firestoreService.AddDocumentAsync(COLLECTION_NAME, product);
                }

                return Ok(new { message = $"{products.Count} ürün başarıyla içe aktarıldı", count = products.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel import sırasında hata oluştu");
                return StatusCode(500, "Excel dosyası içe aktarılırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<ActionResult> ExportToExcel([FromQuery] string? sellerId = null)
        {
            try
            {
                List<Product> products;
                
                if (!string.IsNullOrEmpty(sellerId))
                {
                    products = await _firestoreService.QueryDocumentsAsync<Product>(COLLECTION_NAME, "SellerId", sellerId);
                }
                else
                {
                    products = await _firestoreService.GetAllDocumentsAsync<Product>(COLLECTION_NAME);
                }

                var excelBytes = await _excelService.ExportProductsToExcel(products);
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"products_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel export sırasında hata oluştu");
                return StatusCode(500, "Excel dosyası oluşturulurken bir hata oluştu");
            }
        }

        [HttpGet("template")]
        public async Task<ActionResult> DownloadTemplate()
        {
            try
            {
                var excelBytes = await _excelService.GenerateProductTemplate();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "product_template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Template oluşturulurken hata oluştu");
                return StatusCode(500, "Şablon dosyası oluşturulurken bir hata oluştu");
            }
        }
    }
}
