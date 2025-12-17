namespace EcommerceAPI.DTOs
{
    public class OrderDto
    {
        public string? Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public double TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string ShippingAddress { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }
    
    public class CreateOrderDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
