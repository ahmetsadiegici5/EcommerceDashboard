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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;
        private readonly SellerSettings _sellerSettings;

        public OrdersController(
            IOrderService orderService, 
            ICurrentUserService currentUserService,
            IOptions<SellerSettings> sellerOptions)
        {
            _orderService = orderService;
            _currentUserService = currentUserService;
            _sellerSettings = sellerOptions.Value;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var sellerId = _currentUserService.GetUserIdOrThrow();
            var orders = await _orderService.GetOrdersForSellerAsync(sellerId, page, pageSize);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
                return NotFound($"ID: {id} olan sipariş bulunamadı");

            if (!UserOwnsOrder(order))
                return Forbid();

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto orderDto)
        {
            // SellerId'yi token'dan al
            orderDto.SellerId = _currentUserService.GetUserIdOrThrow();

            var createdOrder = await _orderService.CreateOrderAsync(orderDto);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            var existing = await _orderService.GetOrderByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan sipariş bulunamadı");

            if (!UserOwnsOrder(existing))
                return Forbid();

            await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existing = await _orderService.GetOrderByIdAsync(id);
            if (existing == null)
                return NotFound($"ID: {id} olan sipariş bulunamadı");

            if (!UserOwnsOrder(existing))
                return Forbid();

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

        private bool UserOwnsOrder(OrderDto order)
        {
            if (_sellerSettings.UseSharedSeller)
            {
                return true;
            }

            var currentUserId = _currentUserService.GetUserIdOrThrow();
            return string.Equals(order.SellerId, currentUserId, StringComparison.Ordinal);
        }
    }
}
