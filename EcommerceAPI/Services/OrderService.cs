using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using EcommerceAPI.Configuration;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IFirestoreService _firestoreService;
        private readonly SellerSettings _sellerSettings;
        private const string CollectionName = "Orders";
        private const string ProductCollectionName = "Products";

        public OrderService(IFirestoreService firestoreService, IOptions<SellerSettings> sellerOptions)
        {
            _firestoreService = firestoreService;
            _sellerSettings = sellerOptions.Value;
        }

        public async Task<List<OrderDto>> GetOrdersForSellerAsync(string sellerId, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize;
            List<Order> orders;

            if (_sellerSettings.UseSharedSeller)
            {
                orders = await _firestoreService.GetDocumentsPagedAsync<Order>(
                    CollectionName,
                    pageNumber,
                    pageSize,
                    nameof(Order.CreatedAt),
                    true);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sellerId))
                {
                    throw new ArgumentException("Seller ID is required", nameof(sellerId));
                }

                orders = await _firestoreService.QueryDocumentsAsync<Order>(
                    CollectionName,
                    nameof(Order.SellerId),
                    sellerId,
                    nameof(Order.CreatedAt),
                    true,
                    pageSize,
                    offset);
            }

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string id)
        {
            var order = await _firestoreService.GetDocumentAsync<Order>(CollectionName, id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            if (orderDto == null)
            {
                throw new ArgumentNullException(nameof(orderDto));
            }

            orderDto.SellerId = ResolveSellerId(orderDto.SellerId);

            var db = _firestoreService.GetFirestoreDb();
            var orderRef = db.Collection(CollectionName).Document();

            var createdOrder = await db.RunTransactionAsync(async transaction =>
            {
                var productSnapshots = new Dictionary<string, (Product product, DocumentReference reference)>();

                foreach (var item in orderDto.Items)
                {
                    var productRef = db.Collection(ProductCollectionName).Document(item.ProductId);
                    var snapshot = await transaction.GetSnapshotAsync(productRef);

                    if (!snapshot.Exists)
                    {
                        throw new KeyNotFoundException($"Ürün bulunamadı: {item.ProductId}");
                    }

                    var product = snapshot.ConvertTo<Product>();
                    product.Id = snapshot.Id;

                    if (ShouldEnforceSellerOwnership() &&
                        !string.Equals(product.SellerId, orderDto.SellerId, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Satıcıya ait olmayan ürün için sipariş oluşturulamaz.");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        throw new InvalidOperationException($"Yetersiz stok: {product.Name} - Mevcut: {product.Stock}, İstenen: {item.Quantity}");
                    }

                    productSnapshots[item.ProductId] = (product, productRef);
                }

                var now = DateTime.UtcNow;
                var order = new Order
                {
                    SellerId = orderDto.SellerId,
                    CustomerId = orderDto.CustomerId,
                    CustomerName = orderDto.CustomerName,
                    CustomerEmail = orderDto.CustomerEmail,
                    CustomerPhone = orderDto.CustomerPhone,
                    ShippingAddress = orderDto.ShippingAddress,
                    OrderDate = now,
                    Status = "Pending",
                    OrderNumber = $"ORD-{now.Ticks}",
                    Items = orderDto.Items.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

                transaction.Set(orderRef, order);

                foreach (var item in orderDto.Items)
                {
                    var (product, productRef) = productSnapshots[item.ProductId];
                    var newStock = product.Stock - item.Quantity;

                    var updates = new Dictionary<string, object>
                    {
                        { nameof(Product.Stock), newStock },
                        { nameof(Product.UpdatedAt), now }
                    };

                    transaction.Update(productRef, updates);
                }

                return order;
            });

            createdOrder.Id = orderRef.Id;
            return MapToDto(createdOrder);
        }

        public async Task UpdateOrderStatusAsync(string id, string status)
        {
            var updates = new Dictionary<string, object>
            {
                { nameof(Order.Status), status }
            };
            
            if (status == "Shipped")
            {
                updates.Add(nameof(Order.ShippedDate), DateTime.UtcNow);
            }
            else if (status == "Delivered")
            {
                updates.Add(nameof(Order.DeliveredDate), DateTime.UtcNow);
            }

            await _firestoreService.UpdateDocumentAsync(CollectionName, id, updates);
        }

        public async Task DeleteOrderAsync(string id)
        {
            await _firestoreService.DeleteDocumentAsync(CollectionName, id);
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SellerId = order.SellerId,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                TrackingNumber = order.TrackingNumber,
                OrderDate = order.OrderDate,
                ShippedDate = order.ShippedDate,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }

        private string ResolveSellerId(string? requestedSellerId)
        {
            if (_sellerSettings.UseSharedSeller && !string.IsNullOrWhiteSpace(_sellerSettings.SharedSellerId))
            {
                return _sellerSettings.SharedSellerId;
            }

            return requestedSellerId ?? string.Empty;
        }

        private bool ShouldEnforceSellerOwnership()
        {
            return !_sellerSettings.UseSharedSeller;
        }
    }
}
