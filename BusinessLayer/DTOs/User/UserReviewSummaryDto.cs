using BusinessLayer.DTOs.Review;

namespace BusinessLayer.DTOs.User
{
    public class UserReviewSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }
}
