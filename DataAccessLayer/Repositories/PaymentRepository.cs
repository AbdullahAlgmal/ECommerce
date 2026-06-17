using BusinessLayer.DTOs.Payment;
using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        public new async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    UserId = p.UserId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    UserName = p.User.FirstName + " " + p.User.LastName
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        public async Task<PaymentDto?> GetPaymentByOrderAsync(int orderId)
        {
            return await _dbSet
                .Where(p => p.OrderId == orderId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    UserId = p.UserId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    UserName = p.User.FirstName + " " + p.User.LastName
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.UserId == userId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    UserId = p.UserId,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    UserName = p.User.FirstName + " " + p.User.LastName
                })
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
