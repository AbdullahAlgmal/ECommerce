namespace CoreLayer.DTOs.Review
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string ReviewText { get; set; } = null!;
        public decimal Rating { get; set; }
        public DateOnly ReviewDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
    }
}
