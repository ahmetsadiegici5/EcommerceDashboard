using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Configuration;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly SellerSettings _sellerSettings;

        public ShippingController(
            IShippingService shippingService,
            ICurrentUserService currentUserService,
            IOptions<SellerSettings> sellerOptions)
        {
            _shippingService = shippingService;
            _currentUserService = currentUserService;
            _sellerSettings = sellerOptions.Value;
        }

        [HttpGet]
        public async Task<ActionResult<List<ShippingDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var sellerId = _currentUserService.GetUserIdOrThrow();
            var shippingList = await _shippingService.GetShippingForSellerAsync(sellerId, page, pageSize);
            return Ok(shippingList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShippingDto>> GetById(string id)
        {
            var shipping = await _shippingService.GetShippingByIdAsync(id);
            
            if (shipping == null)
                return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

            if (!UserOwnsShipping(shipping))
                return Forbid();

            return Ok(shipping);
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult<ShippingDto>> GetByTrackingNumber(string trackingNumber)
        {
            var shipping = await _shippingService.GetShippingByTrackingNumberAsync(trackingNumber);
            
            if (shipping == null)
                return NotFound($"Takip numarası: {trackingNumber} olan kargo bulunamadı");

            if (!UserOwnsShipping(shipping))
                return Forbid();

            return Ok(shipping);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ShippingDto>> GetByOrderId(string orderId)
        {
            var shipping = await _shippingService.GetShippingByOrderIdAsync(orderId);
            
            if (shipping == null)
                return NotFound($"Sipariş ID: {orderId} için kargo kaydı bulunamadı");

            if (!UserOwnsShipping(shipping))
                return Forbid();

            return Ok(shipping);
        }

        [HttpPost]
        public async Task<ActionResult<ShippingDto>> Create([FromBody] CreateShippingDto shippingDto)
        {
            shippingDto.SellerId = _currentUserService.GetUserIdOrThrow();

            var createdShipping = await _shippingService.CreateShippingAsync(shippingDto);
            return CreatedAtAction(nameof(GetById), new { id = createdShipping.Id }, createdShipping);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateStatus(string id, [FromBody] UpdateShippingStatusDto statusDto)
        {
            var existing = await _shippingService.GetShippingByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

            if (!UserOwnsShipping(existing))
                return Forbid();

            await _shippingService.UpdateShippingStatusAsync(id, statusDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existing = await _shippingService.GetShippingByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan kargo kaydı bulunamadı");

            if (!UserOwnsShipping(existing))
                return Forbid();

            await _shippingService.DeleteShippingAsync(id);
            return NoContent();
        }

        private bool UserOwnsShipping(ShippingDto shipping)
        {
            if (_sellerSettings.UseSharedSeller)
            {
                return true;
            }

            var currentUserId = _currentUserService.GetUserIdOrThrow();
            return string.Equals(shipping.SellerId, currentUserId, StringComparison.Ordinal);
        }
    }
}
