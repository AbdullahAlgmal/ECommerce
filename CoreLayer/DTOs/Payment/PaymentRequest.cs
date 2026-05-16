namespace CoreLayer.DTOs.Payment
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }
        public string? CardHolderName { get; set; }
    }
}
