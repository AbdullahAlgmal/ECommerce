namespace BusinessLayer.DTOs.OrderItem
{
    public class OrderItemStatisticsDto
    {
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int UniqueProductsSold { get; set; }
        public Dictionary<string, int> TopCategories { get; set; } = new();
    }
}
