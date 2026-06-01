using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.DTOs.Product;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IOrderItemService : IBaseService<OrderItem, OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto>
    {
        Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderAsync(int orderId);
        Task<IEnumerable<OrderItemDto>> GetOrderItemsByProductAsync(int productId);
        Task<decimal> GetTotalSalesByProductAsync(int productId);

        Task<OrderItemStatisticsDto> GetOrderItemStatisticsAsync();
        Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount);

        Task<bool> DeleteOrderItemsByOrderAsync(int orderId);
    }
}
