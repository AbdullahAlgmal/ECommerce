namespace BusinessLayer.DTOs.Order
{
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
        public Dictionary<int, int> MonthlyOrderCounts { get; set; } = new();
    }
}
