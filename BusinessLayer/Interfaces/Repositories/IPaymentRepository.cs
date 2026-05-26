using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> AddAsync(Payment entity);
        Task<Payment> UpdateAsync(Payment entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate);
        Task<Payment?> GetPaymentByOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task<bool> ExistsAsync(Expression<Func<Payment, bool>> predicate);
        Task<decimal> GetTotalPaymentsByUserAsync(int userId);
    }
}
