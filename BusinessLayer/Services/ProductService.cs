using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.ProductImage;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class ProductService : BaseService<Product, ProductDto, CreateProductDto, UpdateProductDto>, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository) : base(productRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public override async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = await MapToDtoList(products);
            return productDtos;
        }
        public async Task<ProductDto?> GetProductWithDetailsAsync(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            return product != null ? await MapToDto(product) : null;
        }
        public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductFilterDto filter)
        {
            var predicate = BuildPredicate(filter);

            var (products, totalCount) = await _productRepository.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                predicate,
                filter.SortBy,
                filter.SortDescending);

            return new PagedResult<ProductDto>
            {
                Items = await Task.WhenAll(products.Select(MapToDto)),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            return await _productRepository.UpdateStockAsync(productId, quantity);
        }
        public async Task<bool> ReduceStockAsync(int productId, int quantity)
        {
            return await _productRepository.ReduceStockAsync(productId, quantity);
        }
        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold)
        {
            var products = await _productRepository.GetLowStockProductsAsync(threshold);
            return await Task.WhenAll(products.Select(MapToDto));
        }
        public async Task<decimal> GetAverageProductPriceAsync()
        {
            return await _productRepository.GetAverageProductPriceAsync();
        }

        private Expression<Func<Product, bool>>? BuildPredicate(ProductFilterDto filter)
        {
            Expression<Func<Product, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                predicate = CombinePredicates(predicate, p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term));
            }

            if (filter.CategoryId.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.Price <= filter.MaxPrice.Value);
            }

            if (filter.InStock.HasValue && filter.InStock.Value)
            {
                predicate = CombinePredicates(predicate, p => p.Quantity > 0);
            }

            return predicate;
        }

        private Expression<Func<Product, bool>>? CombinePredicates(
            Expression<Func<Product, bool>>? existing,
            Expression<Func<Product, bool>> newPredicate)
        {
            if (existing == null)
                return newPredicate;

            var parameter = Expression.Parameter(typeof(Product));
            var combined = Expression.AndAlso(
                Expression.Invoke(existing, parameter),
                Expression.Invoke(newPredicate, parameter));

            return Expression.Lambda<Func<Product, bool>>(combined, parameter);
        }

        protected override Task<ProductDto> MapToDto(Product product)
        {
            return Task.FromResult(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                Images = product.ProductImages?.Select(pi => new ProductImageDto
                {
                    Id = pi.Id,
                    Url = pi.Url,
                    ImageOrder = pi.ImageOrder,
                    ProductId = pi.ProductId,
                    ProductName = product.Name
                }).ToList() ?? new(),
                AverageRating = product.Reviews?.Any() == true ? product.Reviews.Average(r => (double)r.Rating) : 0,
                ReviewCount = product.Reviews?.Count ?? 0
            });
        }
        protected override async Task<IEnumerable<ProductDto>> MapToDtoList(IEnumerable<Product> entities)
        {
            var dtoList = new List<ProductDto>();
            foreach (var entity in entities)
            {
                dtoList.Add(await MapToDto(entity));
            }
            return dtoList;
        }
        protected override Product MapToEntity(CreateProductDto createDto)
        {
            return new Product
            {
                Name = createDto.Name.Trim(),
                Description = createDto.Description?.Trim() ?? string.Empty,
                Price = createDto.Price,
                Quantity = createDto.Quantity,
                CategoryId = createDto.CategoryId
            };
        }
        protected override void UpdateEntity(Product entity, UpdateProductDto updateDto)
        {
            entity.Name = updateDto.Name.Trim();
            entity.Description = updateDto.Description?.Trim() ?? string.Empty;
            entity.Price = updateDto.Price;
            entity.Quantity = updateDto.Quantity;
            entity.CategoryId = updateDto.CategoryId;
        }
        protected override async Task ValidateBeforeCreateAsync(CreateProductDto createDto)
        {
            var isUnique = await _productRepository.IsProductNameUniqueAsync(createDto.Name);
            if (!isUnique)
                throw new InvalidOperationException($"Product name '{createDto.Name}' already exists");

            var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == createDto.CategoryId);
            if (!categoryExists)
                throw new InvalidOperationException($"Category with ID {createDto.CategoryId} does not exist");
        }
        protected override async Task ValidateBeforeUpdateAsync(int id, UpdateProductDto updateDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            var isUnique = await _productRepository.IsProductNameUniqueAsync(updateDto.Name, id);
            if (!isUnique)
                throw new InvalidOperationException($"Product name '{updateDto.Name}' already exists");

            var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == updateDto.CategoryId);
            if (!categoryExists)
                throw new InvalidOperationException($"Category with ID {updateDto.CategoryId} does not exist");
        }
    }
}
