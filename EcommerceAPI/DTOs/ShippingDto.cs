namespace EcommerceAPI.DTOs
{
    public class ShippingDto
    {
        public string? Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string Status { get; set; } = "Preparing";
        public string? CurrentLocation { get; set; }
        public string SellerId { get; set; } = string.Empty;
        public List<ShippingEventDto> Events { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
    }

    public class ShippingEventDto
    {
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CreateShippingDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string SellerId { get; set; } = string.Empty;
    }

    public class UpdateShippingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
