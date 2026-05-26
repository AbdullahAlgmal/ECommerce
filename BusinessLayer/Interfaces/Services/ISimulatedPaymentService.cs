using BusinessLayer.DTOs.Payment;

namespace BusinessLayer.Interfaces.Services
{
    public interface ISimulatedPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount);
    }
}
