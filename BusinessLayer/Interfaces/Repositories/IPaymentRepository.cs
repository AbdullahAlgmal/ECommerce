using BusinessLayer.DTOs.Payment;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        new Task<IEnumerable<PaymentDto>> GetAllAsync();
        Task<PaymentDto?> GetPaymentByOrderAsync(int orderId);
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserAsync(int userId);
        Task<decimal> GetTotalPaymentsByUserAsync(int userId);
    }
}
