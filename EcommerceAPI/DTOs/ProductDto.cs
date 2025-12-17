namespace EcommerceAPI.DTOs
{
    public class ProductDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SellerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SellerId { get; set; } = string.Empty;
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public int? Stock { get; set; }
        public string? Category { get; set; }
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
