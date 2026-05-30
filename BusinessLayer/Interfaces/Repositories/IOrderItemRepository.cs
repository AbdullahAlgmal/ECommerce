using DataAccessLayer;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetItemsByOrderAsync(int orderId);
        Task<bool> DeleteItemsByOrderAsync(int orderId);
        Task<decimal> GetTotalSalesByProductAsync(int productId);
    }
}
