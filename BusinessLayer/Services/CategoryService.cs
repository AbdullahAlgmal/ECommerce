using BusinessLayer.DTOs.Category;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class CategoryService : BaseService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository) : base(categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDto?> GetCategoryWithProductsAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryWithProductsAsync(id);
            return category != null ? await MapToDto(category) : null;
        }
        public async Task<Dictionary<int, int>> GetProductCountPerCategoryAsync()
        {
            return await _categoryRepository.GetProductCountPerCategoryAsync();
        }

        protected override Task<CategoryDto> MapToDto(Category entity)
        {
            return Task.FromResult(new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ProductCount = entity.Products?.Count ?? 0,
                Products = entity.Products?.Select(p => new ProductBriefDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity
                }).ToList() ?? new()
            });
        }
        protected override async Task<IEnumerable<CategoryDto>> MapToDtoList(IEnumerable<Category> entities)
        {
            var categoryDtos = entities.Select(MapToDto);
            return await Task.WhenAll(categoryDtos);
        }
        protected override Category MapToEntity(CreateCategoryDto createDto)
        {
            return new Category
            {
                Name = createDto.Name.Trim(),               
            };
        }
        protected override void UpdateEntity(Category entity, UpdateCategoryDto updateDto)
        {
            entity.Name = updateDto.Name.Trim();
        }
        protected override async Task ValidateBeforeCreateAsync(CreateCategoryDto createDto)
        {
            var isUnique = await _categoryRepository.IsCategoryNameUniqueAsync(createDto.Name);
            if (!isUnique)
                throw new InvalidOperationException($"Category name '{createDto.Name}' already exists");
        }
        protected override async Task ValidateBeforeUpdateAsync(int id, UpdateCategoryDto updateDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found");

            var isUnique = await _categoryRepository.IsCategoryNameUniqueAsync(updateDto.Name, id);
            if (!isUnique)
                throw new InvalidOperationException($"Category name '{updateDto.Name}' already exists");
        }
    }
}
