using CoreLayer.DTOs.Payment;

namespace CoreLayer.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createDto);
        Task<PaymentDto> RefundPaymentAsync(RefundPaymentDto refundDto);
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserAsync(int userId);
        Task<PaymentDto?> GetPaymentByOrderAsync(int orderId);
        Task<decimal> GetTotalUserSpendingAsync(int userId);
    }
}
