using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(
            IProductService productService,
            IOrderService orderService,
            ICurrentUserService currentUserService)
        {
            _productService = productService;
            _orderService = orderService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            var sellerId = _currentUserService.GetUserIdOrThrow();

            // Not: Şimdilik ilk 1000 siparişi dikkate alıyoruz. İleride Firestore tarafında agregasyon
            // ile daha performanslı hale getirilebilir.
            var products = await _productService.GetProductsForSellerAsync(sellerId);
            var orders = await _orderService.GetOrdersForSellerAsync(sellerId, pageNumber: 1, pageSize: 1000);

            var totalProducts = products.Count;
            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var lowStockProducts = products.Count(p => p.Stock < 10);

            var result = new DashboardStatsDto
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                LowStockProducts = lowStockProducts
            };

            return Ok(result);
        }
    }
}


