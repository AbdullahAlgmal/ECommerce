using CoreLayer.DTOs;
using CoreLayer.DTOs.Order;

namespace CoreLayer.Interfaces.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto?> GetOrderWithDetailsAsync(int id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createDto);
        Task<bool> DeleteOrderAsync(int id);

        Task<OrderDto> UpdateOrderStatusAsync(int id, byte newStatus);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(byte status);

        Task<PagedResult<OrderDto>> SearchOrdersAsync(OrderFilterDto filter);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();
        Task<decimal> GetTotalRevenueAsync(DateOnly? fromDate = null, DateOnly? toDate = null);

        Task<bool> CanCancelOrderAsync(int orderId);

        Task<bool> OrderExistsAsync(int id);
        Task<bool> OrderBelongsToUserAsync(int orderId, int userId);
    }
}
