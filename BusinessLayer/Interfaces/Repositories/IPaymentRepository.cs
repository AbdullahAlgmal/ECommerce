using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<Payment?> GetPaymentByOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task<decimal> GetTotalPaymentsByUserAsync(int userId);
    }
}
