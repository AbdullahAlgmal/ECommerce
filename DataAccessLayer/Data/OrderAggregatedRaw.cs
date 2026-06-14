namespace DataAccessLayer.Data
{
    public class OrderAggregatedRaw
    {
        public int OrderId { get; set; }
        public DateOnly OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public int TotalItems { get; set; }
        public string? OrderItemsJson { get; set; }
    }
}
