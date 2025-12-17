using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetOrdersForSellerAsync(string sellerId, int pageNumber, int pageSize);
        Task<OrderDto?> GetOrderByIdAsync(string id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto);
        Task UpdateOrderStatusAsync(string id, string status);
        Task DeleteOrderAsync(string id);
    }
}
