using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface IShippingService
    {
        Task<List<ShippingDto>> GetShippingForSellerAsync(string sellerId, int pageNumber, int pageSize);
        Task<ShippingDto?> GetShippingByIdAsync(string id);
        Task<ShippingDto?> GetShippingByTrackingNumberAsync(string trackingNumber);
        Task<ShippingDto?> GetShippingByOrderIdAsync(string orderId);
        Task<ShippingDto> CreateShippingAsync(CreateShippingDto shippingDto);
        Task UpdateShippingStatusAsync(string id, UpdateShippingStatusDto statusDto);
        Task DeleteShippingAsync(string id);
    }
}
