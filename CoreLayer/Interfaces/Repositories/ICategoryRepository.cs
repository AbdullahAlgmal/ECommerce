using DataAccessLayer;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> AddAsync(Category entity);
        Task<Category> UpdateAsync(Category entity);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate);
        Task<Category?> GetCategoryWithProductsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);

        Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null);
        Task<Dictionary<int, int>> GetProductCountPerCategoryAsync();
    }
}
