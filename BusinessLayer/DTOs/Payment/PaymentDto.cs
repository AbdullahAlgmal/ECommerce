namespace BusinessLayer.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public DateOnly PaymentDate { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? TransactionId { get; set; }
    }
}
