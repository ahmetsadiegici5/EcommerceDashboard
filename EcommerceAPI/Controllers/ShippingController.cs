using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly FirestoreService _firestoreService;
        private readonly ExcelService _excelService;
        private readonly ILogger<ShippingController> _logger;
        private const string COLLECTION_NAME = "shipping";

        public ShippingController(
            FirestoreService firestoreService,
            ExcelService excelService,
            ILogger<ShippingController> logger)
        {
            _firestoreService = firestoreService;
            _excelService = excelService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Shipping>>> GetAll()
        {
            try
            {
                var shippingList = await _firestoreService.GetAllDocumentsAsync<Shipping>(COLLECTION_NAME);
                return Ok(shippingList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo bilgileri getirilirken hata oluştu");
                return StatusCode(500, "Kargo bilgileri getirilirken bir hata oluştu");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Shipping>> GetById(string id)
        {
            try
            {
                var shipping = await _firestoreService.GetDocumentAsync<Shipping>(COLLECTION_NAME, id);
                
                if (shipping == null)
                    return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

                return Ok(shipping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo bilgisi getirilirken hata oluştu");
                return StatusCode(500, "Kargo bilgisi getirilirken bir hata oluştu");
            }
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult<Shipping>> GetByTrackingNumber(string trackingNumber)
        {
            try
            {
                var shippingList = await _firestoreService.QueryDocumentsAsync<Shipping>(
                    COLLECTION_NAME, "TrackingNumber", trackingNumber);
                
                if (shippingList == null || !shippingList.Any())
                    return NotFound($"Takip numarası: {trackingNumber} olan kargo bulunamadı");

                return Ok(shippingList.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo takibi getirilirken hata oluştu");
                return StatusCode(500, "Kargo takibi getirilirken bir hata oluştu");
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<Shipping>> GetByOrderId(string orderId)
        {
            try
            {
                var shippingList = await _firestoreService.QueryDocumentsAsync<Shipping>(
                    COLLECTION_NAME, "OrderId", orderId);
                
                if (shippingList == null || !shippingList.Any())
                    return NotFound($"Sipariş ID: {orderId} için kargo kaydı bulunamadı");

                return Ok(shippingList.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş kargo bilgisi getirilirken hata oluştu");
                return StatusCode(500, "Sipariş kargo bilgisi getirilirken bir hata oluştu");
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] Shipping shipping)
        {
            try
            {
                shipping.CreatedAt = DateTime.UtcNow;
                
                var id = await _firestoreService.AddDocumentAsync(COLLECTION_NAME, shipping);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo kaydı eklenirken hata oluştu");
                return StatusCode(500, "Kargo kaydı eklenirken bir hata oluştu");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] Shipping shipping)
        {
            try
            {
                var existing = await _firestoreService.GetDocumentAsync<Shipping>(COLLECTION_NAME, id);
                if (existing == null)
                    return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

                shipping.Id = id;
                shipping.CreatedAt = existing.CreatedAt;
                
                await _firestoreService.UpdateDocumentAsync(COLLECTION_NAME, id, shipping);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo kaydı güncellenirken hata oluştu");
                return StatusCode(500, "Kargo kaydı güncellenirken bir hata oluştu");
            }
        }

        [HttpPost("{id}/events")]
        public async Task<ActionResult> AddEvent(string id, [FromBody] ShippingEvent shippingEvent)
        {
            try
            {
                var shipping = await _firestoreService.GetDocumentAsync<Shipping>(COLLECTION_NAME, id);
                if (shipping == null)
                    return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

                shippingEvent.Timestamp = DateTime.UtcNow;
                shipping.Events.Add(shippingEvent);
                shipping.Status = shippingEvent.Status;
                shipping.CurrentLocation = shippingEvent.Location;

                await _firestoreService.UpdateDocumentAsync(COLLECTION_NAME, id, shipping);
                return Ok(new { message = "Kargo durumu güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo durumu eklenirken hata oluştu");
                return StatusCode(500, "Kargo durumu eklenirken bir hata oluştu");
            }
        }

        // Excel İşlemleri
        [HttpPost("import")]
        public async Task<ActionResult> ImportFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Dosya seçilmedi");

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                    return BadRequest("Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir");

                using var stream = file.OpenReadStream();
                var shippingList = await _excelService.ImportShippingFromExcel(stream);

                foreach (var shipping in shippingList)
                {
                    await _firestoreService.AddDocumentAsync(COLLECTION_NAME, shipping);
                }

                return Ok(new { message = $"{shippingList.Count} kargo kaydı başarıyla içe aktarıldı", count = shippingList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel import sırasında hata oluştu");
                return StatusCode(500, "Excel dosyası içe aktarılırken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<ActionResult> ExportToExcel()
        {
            try
            {
                var shippingList = await _firestoreService.GetAllDocumentsAsync<Shipping>(COLLECTION_NAME);
                var excelBytes = await _excelService.ExportShippingToExcel(shippingList);
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"shipping_{DateTime.Now:yyyyMMdd}.xlsx");
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
                var excelBytes = await _excelService.GenerateShippingTemplate();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "shipping_template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Template oluşturulurken hata oluştu");
                return StatusCode(500, "Şablon dosyası oluşturulurken bir hata oluştu");
            }
        }
    }
}
