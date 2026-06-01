 using BusinessLayer.DTOs.ProductImage;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessLayer.Services
{
    public class ProductImageService : BaseService<ProductImage, ProductImageDto, CreateProductImageDto, UpdateProductImageDto>, IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;

        public ProductImageService(
            IProductImageRepository productImageRepository,
            IProductRepository productRepository) : base(productImageRepository)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
        }

        public override async Task<ProductImageDto> CreateAsync(CreateProductImageDto createDto)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == createDto.ProductId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {createDto.ProductId} does not exist");

            byte imageOrder;
            if (createDto.ImageOrder.HasValue)
            {
                imageOrder = createDto.ImageOrder.Value;
            }
            else
            {
                var existingImages = await _productImageRepository.GetImagesByProductAsync(createDto.ProductId);
                imageOrder = (byte)(existingImages.Count() + 1);
            }

            var existingCount = await GetImageCountByProductAsync(createDto.ProductId);
            var isFirstImage = existingCount == 0;

            var image = new ProductImage
            {
                Url = createDto.Url,
                ImageOrder = imageOrder,
                ProductId = createDto.ProductId
            };

            var createdImage = await _productImageRepository.AddAsync(image);

            if (isFirstImage && imageOrder != 1)
            {
                createdImage.ImageOrder = 1;
                await _productImageRepository.UpdateAsync(createdImage);
            }

            return await MapToDto(createdImage);
        }
        public override async Task<ProductImageDto> UpdateAsync(int id, UpdateProductImageDto updateDto)
        {
            var image = await _productImageRepository.GetByIdAsync(id);
            if (image == null)
                throw new KeyNotFoundException($"Image with ID {id} not found");

            var oldOrder = image.ImageOrder;
            var newOrder = updateDto.ImageOrder;

            image.Url = updateDto.Url;
            image.ImageOrder = newOrder;

            if (oldOrder != newOrder)
            {
                var productImages = await _productImageRepository.GetImagesByProductAsync(image.ProductId);

                if (newOrder < oldOrder)
                {
                    foreach (var img in productImages.Where(i => i.Id != id && i.ImageOrder >= newOrder && i.ImageOrder < oldOrder))
                    {
                        img.ImageOrder = (byte)(img.ImageOrder + 1);
                        await _productImageRepository.UpdateAsync(img);
                    }
                }
                else if (newOrder > oldOrder)
                {
                    foreach (var img in productImages.Where(i => i.Id != id && i.ImageOrder > oldOrder && i.ImageOrder <= newOrder))
                    {
                        img.ImageOrder = (byte)(img.ImageOrder - 1);
                        await _productImageRepository.UpdateAsync(img);
                    }
                }
            }

            var updatedImage = await _productImageRepository.UpdateAsync(image);
            return await MapToDto(updatedImage);
        }
        public override async Task<bool> DeleteAsync(int id)
        {
            var image = await _productImageRepository.GetByIdAsync(id);
            if (image == null)
                return false;

            var productId = image.ProductId;
            var deletedOrder = image.ImageOrder;

            var deleted = await _productImageRepository.DeleteAsync(id);

            if (deleted)
            {
                var remainingImages = await _productImageRepository.GetImagesByProductAsync(productId);
                foreach (var img in remainingImages.Where(i => i.ImageOrder > deletedOrder))
                {
                    img.ImageOrder = (byte)(img.ImageOrder - 1);
                    await _productImageRepository.UpdateAsync(img);
                }
            }

            return deleted;
        }
        public async Task<IEnumerable<ProductImageDto>> GetImagesByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var images = await _productImageRepository.GetImagesByProductAsync(productId);
            return await MapToDtoList(images);
        }
        public async Task<ProductImageDto?> GetPrimaryImageAsync(int productId)
        {
            var image = await _productImageRepository.GetPrimaryImageAsync(productId);
            return image != null ? await MapToDto(image) : null;
        }
        public async Task<ProductImageDto> SetAsPrimaryAsync(int imageId, int productId)
        {
            var belongsToProduct = await ImageBelongsToProductAsync(imageId, productId);
            if (!belongsToProduct)
                throw new InvalidOperationException($"Image {imageId} does not belong to product {productId}");

            var images = await _productImageRepository.GetImagesByProductAsync(productId);

            byte newOrder = 1;
            foreach (var img in images.OrderBy(i => i.ImageOrder))
            {
                if (img.Id == imageId)
                {
                    img.ImageOrder = 1;
                }
                else
                {
                    newOrder++;
                    img.ImageOrder = newOrder;
                }
                await _productImageRepository.UpdateAsync(img);
            }

            var primaryImage = await _productImageRepository.GetByIdAsync(imageId);
            return await MapToDto(primaryImage!);
        }

        public async Task<IEnumerable<ProductImageDto>> BulkUploadImagesAsync(BulkUploadImagesDto bulkDto)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == bulkDto.ProductId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {bulkDto.ProductId} does not exist");

            var existingImages = await _productImageRepository.GetImagesByProductAsync(bulkDto.ProductId);
            var currentMaxOrder = existingImages.Any() ? existingImages.Max(i => i.ImageOrder) : 0;

            var createdImages = new List<ProductImage>();
            var order = currentMaxOrder;

            foreach (var url in bulkDto.ImageUrls)
            {
                order++;
                var image = new ProductImage
                {
                    Url = url,
                    ImageOrder = (byte)order,
                    ProductId = bulkDto.ProductId
                };
                var created = await _productImageRepository.AddAsync(image);
                createdImages.Add(created);
            }

            if (currentMaxOrder == 0 && createdImages.Any())
            {
                var reorderDict = new Dictionary<int, byte>();
                for (int i = 0; i < createdImages.Count; i++)
                {
                    reorderDict[createdImages[i].Id] = (byte)(i + 1);
                }
                await ReorderImagesAsync(bulkDto.ProductId, reorderDict);
            }

            return await MapToDtoList(createdImages);
        }
        public async Task<bool> DeleteAllProductImagesAsync(int productId)
        {
            return await _productImageRepository.DeleteImagesByProductAsync(productId);
        }
        public async Task<bool> ReorderImagesAsync(int productId, Dictionary<int, byte> imageOrders)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            return await _productImageRepository.ReorderImagesAsync(productId, imageOrders);
        }

        public async Task<bool> ImageBelongsToProductAsync(int imageId, int productId)
        {
            return await _productImageRepository.UserOwnsAddressAsync(imageId, productId);
        }
        public async Task<int> GetImageCountByProductAsync(int productId)
        {
            return await _productImageRepository.CountAddressesByUserAsync(productId);
        }

        protected override Task<ProductImageDto> MapToDto(ProductImage entity)
        {
            return Task.FromResult(new ProductImageDto
            {
                Id = entity.Id,
                Url = entity.Url,
                ImageOrder = entity.ImageOrder,
                ProductId = entity.ProductId,
                ProductName = entity.Product?.Name ?? string.Empty
            });
        }
        protected override async Task<IEnumerable<ProductImageDto>> MapToDtoList(IEnumerable<ProductImage> entities)
        {
            var dtoList = new List<ProductImageDto>();
            foreach (var entity in entities)
            {
                dtoList.Add(await MapToDto(entity));
            }
            return dtoList;
        }
        protected override ProductImage MapToEntity(CreateProductImageDto createDto)
        {
            return new ProductImage
            {
                Url = createDto.Url,
                ImageOrder = createDto.ImageOrder ?? 0,
                ProductId = createDto.ProductId
            };
        }
        protected override void UpdateEntity(ProductImage entity, UpdateProductImageDto updateDto)
        {
            entity.Url = updateDto.Url;
            entity.ImageOrder = updateDto.ImageOrder;
        }
    }
}
