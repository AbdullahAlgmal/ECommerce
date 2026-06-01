using BusinessLayer.DTOs.Category;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface ICategoryService : IBaseService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
    {
        Task<CategoryDto?> GetCategoryWithProductsAsync(int id);
        Task<Dictionary<int, int>> GetProductCountPerCategoryAsync();
    }
}
