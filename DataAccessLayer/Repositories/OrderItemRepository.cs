using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<OrderItem> _dbSet;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<OrderItem>();
        }
        public async Task<OrderItem?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }
        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            return await _dbSet
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ToListAsync();
        }
        public async Task<IEnumerable<OrderItem>> GetItemsByOrderAsync(int orderId)
        {
            return await _dbSet
                .Include(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }
        public async Task<OrderItem> AddAsync(OrderItem entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<OrderItem> UpdateAsync(OrderItem entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item == null)
                return false;

            _dbSet.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteItemsByOrderAsync(int orderId)
        {
            var items = await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync();
            if (!items.Any())
                return false;

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<decimal> GetTotalSalesByProductAsync(int productId)
        {
            return await _dbSet
                .Where(oi => oi.ProductId == productId)
                .SumAsync(oi => oi.TotalPrice);
        }
    }
}
