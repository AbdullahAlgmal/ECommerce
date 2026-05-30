using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(int id);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);
        Task<Dictionary<int, int>> GetProductCountPerCategoryAsync();
    }
}
