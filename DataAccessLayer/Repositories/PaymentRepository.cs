using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Payment> _dbSet;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Payment>();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Order)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Order)
                .Include(p => p.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        public async Task<Payment> AddAsync(Payment entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Payment> UpdateAsync(Payment entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await GetByIdAsync(id);
            if (payment == null)
                return false;

            _dbSet.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet
                .Include(p => p.Order)
                .Include(p => p.User)
                .Where(predicate)
                .ToListAsync();
        }
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
        public async Task<bool> ExistsAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<decimal> GetTotalPaymentsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId && p.Status == "Completed")
                .SumAsync(p => p.Amount);
        }
    }
}
