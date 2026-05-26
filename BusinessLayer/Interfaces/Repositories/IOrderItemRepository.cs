using DataAccessLayer;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(int id);
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<IEnumerable<OrderItem>> GetItemsByOrderAsync(int orderId);
        Task<OrderItem> AddAsync(OrderItem entity);
        Task<OrderItem> UpdateAsync(OrderItem entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteItemsByOrderAsync(int orderId);
        Task<decimal> GetTotalSalesByProductAsync(int productId);
    }
}
