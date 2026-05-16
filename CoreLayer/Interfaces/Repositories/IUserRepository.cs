using ECommerceApi;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User> AddAsync(User entity);
        Task<User> UpdateAsync(User entity);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate);
        Task<User?> GetUserWithAddressesAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
        Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
        Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null);

        Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<User, bool>>? predicate = null);
    }
}
