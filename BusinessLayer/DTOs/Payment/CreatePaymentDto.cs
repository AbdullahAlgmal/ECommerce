using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Payment
{
    public class CreatePaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Method { get; set; } = null!;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        // Payment details for simulation
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }
        public string? CardHolderName { get; set; }
    }
}
