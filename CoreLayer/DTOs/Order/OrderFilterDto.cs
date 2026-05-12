namespace CoreLayer.DTOs.Order
{
    public class OrderFilterDto
    {
        public int? UserId { get; set; }
        public byte? Status { get; set; }
        public DateOnly? OrderDateFrom { get; set; }
        public DateOnly? OrderDateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
    }
}
