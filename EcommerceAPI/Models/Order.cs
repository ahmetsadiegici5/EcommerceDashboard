using Google.Cloud.Firestore;

namespace EcommerceAPI.Models
{
    [FirestoreData]
    public class Order
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string OrderNumber { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SellerId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CustomerId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CustomerName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CustomerEmail { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CustomerPhone { get; set; } = string.Empty;

        [FirestoreProperty]
        public List<OrderItem> Items { get; set; } = new();

        [FirestoreProperty]
        public double TotalAmount { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

        [FirestoreProperty]
        public string ShippingAddress { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? TrackingNumber { get; set; }

        [FirestoreProperty]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? ShippedDate { get; set; }

        [FirestoreProperty]
        public DateTime? DeliveredDate { get; set; }
    }

    [FirestoreData]
    public class OrderItem
    {
        [FirestoreProperty]
        public string ProductId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ProductName { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public double UnitPrice { get; set; }

        [FirestoreProperty]
        public double TotalPrice { get; set; }
    }
}
