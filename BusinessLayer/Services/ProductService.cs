using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.ProductImage;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToProductDto);
        }
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToProductDto(product) : null;
        }
        public async Task<ProductDto?> GetProductWithDetailsAsync(int id)
        {
            var product = await _productRepository.GetProductWithDetailsAsync(id);
            return product != null ? MapToProductDto(product) : null;
        }
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            var isUnique = await _productRepository.IsProductNameUniqueAsync(createDto.Name);
            if (!isUnique)
                throw new InvalidOperationException($"Product name '{createDto.Name}' already exists");

            var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == createDto.CategoryId);
            if (!categoryExists)
                throw new InvalidOperationException($"Category with ID {createDto.CategoryId} does not exist");

            var product = new Product
            {
                Name = createDto.Name.Trim(),
                Description = createDto.Description?.Trim() ?? string.Empty,
                Price = createDto.Price,
                Quantity = createDto.Quantity,
                CategoryId = createDto.CategoryId
            };

            var created = await _productRepository.AddAsync(product);
            return MapToProductDto(created);
        }
        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto)
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

            product.Name = updateDto.Name.Trim();
            product.Description = updateDto.Description?.Trim() ?? string.Empty;
            product.Price = updateDto.Price;
            product.Quantity = updateDto.Quantity;
            product.CategoryId = updateDto.CategoryId;

            var updated = await _productRepository.UpdateAsync(product);
            return MapToProductDto(updated);
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
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
                Items = products.Select(MapToProductDto),
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
            return products.Select(MapToProductDto);
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

        private ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
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
            };
        }
    }
}
