namespace BusinessLayer.DTOs.Product
{
    public class ProductSalesDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumberOfOrders { get; set; }
    }
}
