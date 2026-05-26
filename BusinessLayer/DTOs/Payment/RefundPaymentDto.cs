using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Payment
{
    public class RefundPaymentDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        public string? Reason { get; set; }
    }
}
