namespace CoreLayer.DTOs.Review
{
    public class ReviewFilterDto
    {
        public int? ProductId { get; set; }
        public int? UserId { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        public DateOnly? ReviewDateFrom { get; set; }
        public DateOnly? ReviewDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "ReviewDate";
        public bool SortDescending { get; set; } = true;
    }
}
