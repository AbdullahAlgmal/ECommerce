using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.DTOs.Product;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        Task<IEnumerable<OrderItemDto>> GetItemsByOrderAsync(int orderId);
        Task<IEnumerable<OrderItemDto>> GetItemsByProductAsync(int productId);
        Task<OrderItemStatisticsDto> GetStatisticsAsync();
        Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount);
        Task<bool> DeleteItemsByOrderAsync(int orderId);
        Task<decimal> GetTotalSalesByProductAsync(int productId);
    }
}
