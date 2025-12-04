using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly FirestoreService _firestoreService;
        private readonly ILogger<OrdersController> _logger;
        private const string COLLECTION_NAME = "orders";

        public OrdersController(FirestoreService firestoreService, ILogger<OrdersController> logger)
        {
            _firestoreService = firestoreService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetAll([FromQuery] string? sellerId = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(sellerId))
                {
                    var orders = await _firestoreService.QueryDocumentsAsync<Order>(COLLECTION_NAME, "SellerId", sellerId);
                    return Ok(orders);
                }

                var allOrders = await _firestoreService.GetAllDocumentsAsync<Order>(COLLECTION_NAME);
                return Ok(allOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler getirilirken hata oluştu");
                return StatusCode(500, "Siparişler getirilirken bir hata oluştu");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetById(string id)
        {
            try
            {
                var order = await _firestoreService.GetDocumentAsync<Order>(COLLECTION_NAME, id);
                
                if (order == null)
                    return NotFound($"ID: {id} olan sipariş bulunamadı");

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş getirilirken hata oluştu");
                return StatusCode(500, "Sipariş getirilirken bir hata oluştu");
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] Order order)
        {
            try
            {
                // Sipariş oluşturma öncesi stok kontrolü
                foreach (var item in order.Items)
                {
                    var product = await _firestoreService.GetDocumentAsync<Product>("products", item.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Ürün bulunamadı: {item.ProductId}");
                    }
                    
                    if (product.Stock < item.Quantity)
                    {
                        return BadRequest($"Yetersiz stok: {product.Name} - Mevcut: {product.Stock}, İstenen: {item.Quantity}");
                    }
                }

                // Sipariş oluştur
                order.OrderDate = DateTime.UtcNow;
                order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
                var id = await _firestoreService.AddDocumentAsync(COLLECTION_NAME, order);

                // Stok güncelle
                foreach (var item in order.Items)
                {
                    var product = await _firestoreService.GetDocumentAsync<Product>("products", item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        await _firestoreService.UpdateDocumentAsync("products", item.ProductId, product);
                        _logger.LogInformation($"Ürün stoğu güncellendi: {product.Name}, Yeni stok: {product.Stock}");
                    }
                }

                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş eklenirken hata oluştu");
                return StatusCode(500, "Sipariş eklenirken bir hata oluştu");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] Order order)
        {
            try
            {
                var existing = await _firestoreService.GetDocumentAsync<Order>(COLLECTION_NAME, id);
                if (existing == null)
                    return NotFound($"ID: {id} olan sipariş bulunamadı");

                order.Id = id;
                order.OrderDate = existing.OrderDate;
                order.OrderNumber = existing.OrderNumber;
                
                await _firestoreService.UpdateDocumentAsync(COLLECTION_NAME, id, order);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş güncellenirken hata oluştu");
                return StatusCode(500, "Sipariş güncellenirken bir hata oluştu");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateStatus(string id, [FromBody] StatusUpdateRequest request)
        {
            try
            {
                var order = await _firestoreService.GetDocumentAsync<Order>(COLLECTION_NAME, id);
                if (order == null)
                    return NotFound($"ID: {id} olan sipariş bulunamadı");

                order.Status = request.Status;
                
                if (request.Status == "Shipped")
                {
                    order.ShippedDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(request.TrackingNumber))
                        order.TrackingNumber = request.TrackingNumber;
                }
                else if (request.Status == "Delivered")
                {
                    order.DeliveredDate = DateTime.UtcNow;
                }

                await _firestoreService.UpdateDocumentAsync(COLLECTION_NAME, id, order);
                return Ok(new { message = "Sipariş durumu güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken hata oluştu");
                return StatusCode(500, "Sipariş durumu güncellenirken bir hata oluştu");
            }
        }
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
    }
}
