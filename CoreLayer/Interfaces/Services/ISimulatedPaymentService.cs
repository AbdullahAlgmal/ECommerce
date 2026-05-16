using CoreLayer.DTOs.Payment;

namespace CoreLayer.Interfaces.Services
{
    public interface ISimulatedPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount);
    }
}
