namespace DataAccessLayer.Data
{
    public class OrderItemStatisticsRaw
    {
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int UniqueProductsSold { get; set; }
        public string? TopCategoriesJson { get; set; }
    }
}
