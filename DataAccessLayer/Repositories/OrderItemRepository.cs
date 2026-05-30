using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {

        public OrderItemRepository(AppDbContext context) : base(context) { }
        public async Task<IEnumerable<OrderItem>> GetItemsByOrderAsync(int orderId)
        {
            return await _dbSet
                .Include(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
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
