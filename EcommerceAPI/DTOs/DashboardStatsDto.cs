namespace EcommerceAPI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
        public int LowStockProducts { get; set; }
    }
}


