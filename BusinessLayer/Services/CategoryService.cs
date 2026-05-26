using BusinessLayer.DTOs.Category;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToCategoryDto);
        }
        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? MapToCategoryDto(category) : null;
        }
        public async Task<CategoryDto?> GetCategoryWithProductsAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryWithProductsAsync(id);
            return category != null ? MapToCategoryDto(category) : null;
        }
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            var isUnique = await _categoryRepository.IsCategoryNameUniqueAsync(createDto.Name);
            if (!isUnique)
                throw new InvalidOperationException($"Category name '{createDto.Name}' already exists");

            var category = new Category
            {
                Name = createDto.Name.Trim()
            };

            var created = await _categoryRepository.AddAsync(category);
            return MapToCategoryDto(created);
        }
        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found");

            var isUnique = await _categoryRepository.IsCategoryNameUniqueAsync(updateDto.Name, id);
            if (!isUnique)
                throw new InvalidOperationException($"Category name '{updateDto.Name}' already exists");

            category.Name = updateDto.Name.Trim();
            var updated = await _categoryRepository.UpdateAsync(category);
            return MapToCategoryDto(updated);
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _categoryRepository.DeleteAsync(id);
        }
        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(c => c.Id == id);
        }
        public async Task<Dictionary<int, int>> GetProductCountPerCategoryAsync()
        {
            return await _categoryRepository.GetProductCountPerCategoryAsync();
        }

        private CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products?.Count ?? 0,
                Products = category.Products?.Select(p => new ProductBriefDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity
                }).ToList() ?? new()
            };
        }
    }
}
