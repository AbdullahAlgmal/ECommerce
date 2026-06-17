using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.DTOs.Product;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class OrderItemService : BaseService<OrderItem, OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto>, IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderItemService(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository) : base(orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public override async Task<OrderItemDto> CreateAsync(CreateOrderItemDto createDto)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == createDto.OrderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {createDto.OrderId} does not exist");

            var product = await _productRepository.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {createDto.ProductId} does not exist");

            if (product.Quantity < createDto.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}");

            var totalPrice = product.Price * createDto.Quantity;

            var orderItem = MapToEntity(createDto);
            orderItem.Price = product.Price;
            orderItem.TotalPrice = totalPrice;

            await _productRepository.ReduceStockAsync(createDto.ProductId, createDto.Quantity);

            var order = await _orderRepository.GetByIdAsync(createDto.OrderId);
            if (order != null)
            {
                order.TotalAmount += totalPrice;
                await _orderRepository.UpdateAsync(order);
            }

            var createdOrderItem = await _orderItemRepository.AddAsync(orderItem);
            return await MapToDto(createdOrderItem);
        }
        public override async Task<OrderItemDto> UpdateAsync(int id, UpdateOrderItemDto updateDto)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(id);
            if (orderItem == null)
                throw new KeyNotFoundException($"Order item with ID {id} not found");

            var quantityDifference = updateDto.Quantity - orderItem.Quantity;
            var priceDifference = updateDto.Price - orderItem.Price;

            if (quantityDifference != 0)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    if (quantityDifference > 0 && product.Quantity < quantityDifference)
                        throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'");

                    if (quantityDifference > 0)
                        await _productRepository.ReduceStockAsync(orderItem.ProductId, quantityDifference);
                    else
                        await _productRepository.UpdateStockAsync(orderItem.ProductId, product.Quantity - quantityDifference);
                }
            }

            UpdateEntity(orderItem, updateDto);

            var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
            if (order != null)
            {
                order.TotalAmount += (orderItem.TotalPrice - (orderItem.Quantity - quantityDifference) * (orderItem.Price - priceDifference));
                await _orderRepository.UpdateAsync(order);
            }

            var updatedOrderItem = await _orderItemRepository.UpdateAsync(orderItem);
            return await MapToDto(updatedOrderItem);
        }

        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderAsync(int orderId)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == orderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {orderId} does not exist");

            var orderItems = await _orderItemRepository.GetItemsByOrderAsync(orderId);
            return orderItems;
        }
        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var productItems = await _orderItemRepository.GetItemsByProductAsync(productId);
            return productItems;
        }
        public async Task<decimal> GetTotalSalesByProductAsync(int productId)
        {
            return await _orderItemRepository.GetTotalSalesByProductAsync(productId);
        }

        public async Task<OrderItemStatisticsDto> GetOrderItemStatisticsAsync()
        {
            return await _orderItemRepository.GetStatisticsAsync();
        }
        public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount)
        {
            return await _orderItemRepository.GetTopSellingProductsAsync(topCount);
        }
        public async Task<bool> DeleteOrderItemsByOrderAsync(int orderId)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == orderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {orderId} does not exist");

            var orderItems = await _orderItemRepository.GetItemsByOrderAsync(orderId);

            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            return await _orderItemRepository.DeleteItemsByOrderAsync(orderId);
        }

        protected override Task<OrderItemDto> MapToDto(OrderItem entity)
        {
            return Task.FromResult(new OrderItemDto
            {
                Id = entity.Id,
                Quantity = entity.Quantity,
                Price = entity.Price,
                TotalPrice = entity.TotalPrice,
                OrderId = entity.OrderId,
                ProductId = entity.ProductId,
                ProductName = entity.Product?.Name ?? "Unknown",
                ProductImage = entity.Product?.ProductImages?.FirstOrDefault()?.Url ?? string.Empty
            });
        }
        protected override async Task<IEnumerable<OrderItemDto>> MapToDtoList(IEnumerable<OrderItem> entities)
        {
            var dtoTasks = entities.Select(MapToDto);
            return await Task.WhenAll(dtoTasks);
        }
        protected override OrderItem MapToEntity(CreateOrderItemDto createDto)
        {
            return new OrderItem
            {
                OrderId = createDto.OrderId,
                ProductId = createDto.ProductId,
                Quantity = createDto.Quantity,
                Price = 0,
                TotalPrice = 0
            };
        }
        protected override void UpdateEntity(OrderItem entity, UpdateOrderItemDto updateDto)
        {
            entity.Quantity = updateDto.Quantity;
            entity.Price = updateDto.Price;
            entity.TotalPrice = updateDto.Quantity * updateDto.Price;
        }
         protected override async Task ValidateBeforeDeleteAsync(int id)
         {
            var orderItem = await _orderItemRepository.GetByIdAsync(id);
            if (orderItem == null)
                return;

            var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
            if (product != null)
            {
                product.Quantity += orderItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
            if (order != null)
            {
                order.TotalAmount -= orderItem.TotalPrice;
                await _orderRepository.UpdateAsync(order);
            }
        }
    }
}
