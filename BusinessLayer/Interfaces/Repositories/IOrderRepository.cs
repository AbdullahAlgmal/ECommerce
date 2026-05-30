using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(byte status);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateOnly startDate, DateOnly endDate);

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
