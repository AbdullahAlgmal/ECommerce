namespace BusinessLayer.DTOs.Payment
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
