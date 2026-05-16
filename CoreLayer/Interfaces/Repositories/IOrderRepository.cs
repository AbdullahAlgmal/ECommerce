using ECommerceApi;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> AddAsync(Order entity);
        Task<Order> UpdateAsync(Order entity);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(byte status);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateOnly startDate, DateOnly endDate);

        Task<bool> ExistsAsync(Expression<Func<Order, bool>> predicate);

        Task<int> CountAsync(Expression<Func<Order, bool>>? predicate = null);
        Task<decimal> GetTotalRevenueAsync(DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<Dictionary<byte, int>> GetOrderStatusStatisticsAsync();
        Task<Dictionary<int, int>> GetMonthlyOrderCountAsync(int year);

        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Order, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false);
    }
}
