using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.DTOs.Product;

namespace BusinessLayer.Interfaces.Services
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItemDto>> GetAllOrderItemsAsync();
        Task<OrderItemDto?> GetOrderItemByIdAsync(int id);
        Task<OrderItemDto> CreateOrderItemAsync(CreateOrderItemDto createDto);
        Task<OrderItemDto> UpdateOrderItemAsync(int id, UpdateOrderItemDto updateDto);
        Task<bool> DeleteOrderItemAsync(int id);

        Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderAsync(int orderId);
        Task<IEnumerable<OrderItemDto>> GetOrderItemsByProductAsync(int productId);
        Task<decimal> GetTotalSalesByProductAsync(int productId);

        Task<OrderItemStatisticsDto> GetOrderItemStatisticsAsync();
        Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount);

        Task<bool> DeleteOrderItemsByOrderAsync(int orderId);
    }
}
