namespace BusinessLayer.DTOs.Product
{
    public class ProductRatingDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }
}
