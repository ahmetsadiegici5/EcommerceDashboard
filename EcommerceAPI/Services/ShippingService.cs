using System;
using EcommerceAPI.Configuration;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IFirestoreService _firestoreService;
        private readonly SellerSettings _sellerSettings;
        private const string CollectionName = "Shipping";

        public ShippingService(IFirestoreService firestoreService, IOptions<SellerSettings> sellerOptions)
        {
            _firestoreService = firestoreService;
            _sellerSettings = sellerOptions.Value;
        }

        public async Task<List<ShippingDto>> GetShippingForSellerAsync(string sellerId, int pageNumber, int pageSize)
        {
            List<Shipping> shippingList;

            if (_sellerSettings.UseSharedSeller)
            {
                shippingList = await _firestoreService.GetDocumentsPagedAsync<Shipping>(
                    CollectionName,
                    pageNumber,
                    pageSize,
                    nameof(Shipping.CreatedAt),
                    true);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sellerId))
                {
                    throw new ArgumentException("Seller ID is required", nameof(sellerId));
                }

                var offset = (pageNumber - 1) * pageSize;
                shippingList = await _firestoreService.QueryDocumentsAsync<Shipping>(
                    CollectionName,
                    nameof(Shipping.SellerId),
                    sellerId,
                    nameof(Shipping.CreatedAt),
                    true,
                    pageSize,
                    offset);
            }

            return shippingList.Select(MapToDto).ToList();
        }

        public async Task<ShippingDto?> GetShippingByIdAsync(string id)
        {
            var shipping = await _firestoreService.GetDocumentAsync<Shipping>(CollectionName, id);
            return shipping != null ? MapToDto(shipping) : null;
        }

        public async Task<ShippingDto?> GetShippingByTrackingNumberAsync(string trackingNumber)
        {
            var shippingList = await _firestoreService.QueryDocumentsAsync<Shipping>(CollectionName, nameof(Shipping.TrackingNumber), trackingNumber);
            return shippingList.FirstOrDefault() != null ? MapToDto(shippingList.First()) : null;
        }

        public async Task<ShippingDto?> GetShippingByOrderIdAsync(string orderId)
        {
            var shippingList = await _firestoreService.QueryDocumentsAsync<Shipping>(CollectionName, nameof(Shipping.OrderId), orderId);
            return shippingList.FirstOrDefault() != null ? MapToDto(shippingList.First()) : null;
        }

        public async Task<ShippingDto> CreateShippingAsync(CreateShippingDto shippingDto)
        {
            if (shippingDto == null) throw new ArgumentNullException(nameof(shippingDto));
            if (string.IsNullOrEmpty(shippingDto.OrderId)) throw new ArgumentException("OrderId cannot be empty");

            var shipping = new Shipping
            {
                OrderId = shippingDto.OrderId,
                Carrier = shippingDto.Carrier,
                TrackingNumber = shippingDto.TrackingNumber ?? Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                Status = "Preparing",
                SellerId = ResolveSellerId(shippingDto.SellerId),
                CreatedAt = DateTime.UtcNow,
                Events = new List<ShippingEvent>
                {
                    new ShippingEvent
                    {
                        Status = "Preparing",
                        Location = "Warehouse",
                        Description = "Shipping label created",
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            var id = await _firestoreService.AddDocumentAsync(CollectionName, shipping);
            shipping.Id = id;
            return MapToDto(shipping);
        }

        private string ResolveSellerId(string? requestedSellerId)
        {
            if (_sellerSettings.UseSharedSeller && !string.IsNullOrWhiteSpace(_sellerSettings.SharedSellerId))
            {
                return _sellerSettings.SharedSellerId;
            }

            return requestedSellerId ?? string.Empty;
        }

        public async Task UpdateShippingStatusAsync(string id, UpdateShippingStatusDto statusDto)
        {
            var shipping = await _firestoreService.GetDocumentAsync<Shipping>(CollectionName, id);
            if (shipping == null) throw new KeyNotFoundException($"Shipping record with ID {id} not found");

            var newEvent = new ShippingEvent
            {
                Status = statusDto.Status,
                Location = statusDto.Location,
                Description = statusDto.Description,
                Timestamp = DateTime.UtcNow
            };

            shipping.Events.Add(newEvent);
            shipping.Status = statusDto.Status;
            shipping.CurrentLocation = statusDto.Location;

            if (statusDto.Status == "Delivered")
            {
                shipping.ActualDeliveryDate = DateTime.UtcNow;
            }

            var updates = new Dictionary<string, object>
            {
                { nameof(Shipping.Status), shipping.Status },
                { nameof(Shipping.CurrentLocation), shipping.CurrentLocation },
                { nameof(Shipping.Events), shipping.Events }
            };

            if (shipping.ActualDeliveryDate.HasValue)
            {
                updates.Add(nameof(Shipping.ActualDeliveryDate), shipping.ActualDeliveryDate.Value);
            }

            await _firestoreService.UpdateDocumentAsync(CollectionName, id, updates);
        }

        public async Task DeleteShippingAsync(string id)
        {
            await _firestoreService.DeleteDocumentAsync(CollectionName, id);
        }

        private static ShippingDto MapToDto(Shipping shipping)
        {
            return new ShippingDto
            {
                Id = shipping.Id,
                OrderId = shipping.OrderId,
                TrackingNumber = shipping.TrackingNumber,
                Carrier = shipping.Carrier,
                Status = shipping.Status,
                CurrentLocation = shipping.CurrentLocation,
                SellerId = shipping.SellerId,
                CreatedAt = shipping.CreatedAt,
                EstimatedDeliveryDate = shipping.EstimatedDeliveryDate,
                ActualDeliveryDate = shipping.ActualDeliveryDate,
                Events = shipping.Events.Select(e => new ShippingEventDto
                {
                    Status = e.Status,
                    Location = e.Location,
                    Description = e.Description,
                    Timestamp = e.Timestamp
                }).ToList()
            };
        }
    }
}
