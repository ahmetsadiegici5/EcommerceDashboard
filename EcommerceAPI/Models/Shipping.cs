using Google.Cloud.Firestore;

namespace EcommerceAPI.Models
{
    [FirestoreData]
    public class Shipping
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string OrderId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string TrackingNumber { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Carrier { get; set; } = string.Empty; // Aras Kargo, Yurtiçi Kargo, MNG, etc.

        [FirestoreProperty]
        public string Status { get; set; } = "Preparing"; // Preparing, Shipped, InTransit, OutForDelivery, Delivered

        [FirestoreProperty]
        public string SellerId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? CurrentLocation { get; set; }

        [FirestoreProperty]
        public List<ShippingEvent> Events { get; set; } = new();

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [FirestoreProperty]
        public DateTime? ActualDeliveryDate { get; set; }
    }

    [FirestoreData]
    public class ShippingEvent
    {
        [FirestoreProperty]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
