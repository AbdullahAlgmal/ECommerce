using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        public async Task<Payment?> GetPaymentByOrderAsync(int orderId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.Order)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        public async Task<decimal> GetTotalPaymentsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.Status == "Completed")
                .SumAsync(p => p.Amount);
        }
    }
}
