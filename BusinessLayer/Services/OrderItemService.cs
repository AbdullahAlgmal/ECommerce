using CoreLayer.DTOs.OrderItem;
using CoreLayer.DTOs.Product;
using CoreLayer.Interfaces.Repositories;
using CoreLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderItemService(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderItemDto>> GetAllOrderItemsAsync()
        {
            var orderItems = await _orderItemRepository.GetAllAsync();
            return orderItems.Select(MapToOrderItemDto);
        }
        public async Task<OrderItemDto?> GetOrderItemByIdAsync(int id)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(id);
            return orderItem != null ? MapToOrderItemDto(orderItem) : null;
        }
        public async Task<OrderItemDto> CreateOrderItemAsync(CreateOrderItemDto createDto)
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

            var orderItem = new OrderItem
            {
                OrderId = createDto.OrderId,
                ProductId = createDto.ProductId,
                Quantity = createDto.Quantity,
                Price = product.Price,
                TotalPrice = totalPrice
            };

            await _productRepository.ReduceStockAsync(createDto.ProductId, createDto.Quantity);

            var order = await _orderRepository.GetByIdAsync(createDto.OrderId);
            if (order != null)
            {
                order.TotalAmount += totalPrice;
                await _orderRepository.UpdateAsync(order);
            }

            var createdOrderItem = await _orderItemRepository.AddAsync(orderItem);
            return MapToOrderItemDto(createdOrderItem);
        }
        public async Task<OrderItemDto> UpdateOrderItemAsync(int id, UpdateOrderItemDto updateDto)
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

            orderItem.Quantity = updateDto.Quantity;
            orderItem.Price = updateDto.Price;
            orderItem.TotalPrice = updateDto.Quantity * updateDto.Price;

            var order = await _orderRepository.GetByIdAsync(orderItem.OrderId);
            if (order != null)
            {
                order.TotalAmount += (orderItem.TotalPrice - (orderItem.Quantity - quantityDifference) * (orderItem.Price - priceDifference));
                await _orderRepository.UpdateAsync(order);
            }

            var updatedOrderItem = await _orderItemRepository.UpdateAsync(orderItem);
            return MapToOrderItemDto(updatedOrderItem);
        }
        public async Task<bool> DeleteOrderItemAsync(int id)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(id);
            if (orderItem == null)
                return false;

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

            return await _orderItemRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderAsync(int orderId)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == orderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {orderId} does not exist");

            var orderItems = await _orderItemRepository.GetItemsByOrderAsync(orderId);
            return orderItems.Select(MapToOrderItemDto);
        }
        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var allItems = await _orderItemRepository.GetAllAsync();
            var productItems = allItems.Where(oi => oi.ProductId == productId);
            return productItems.Select(MapToOrderItemDto);
        }
        public async Task<decimal> GetTotalSalesByProductAsync(int productId)
        {
            return await _orderItemRepository.GetTotalSalesByProductAsync(productId);
        }

        public async Task<OrderItemStatisticsDto> GetOrderItemStatisticsAsync()
        {
            var allItems = await _orderItemRepository.GetAllAsync();
            var itemsList = allItems.ToList();

            var totalItemsSold = itemsList.Sum(i => i.Quantity);
            var totalRevenue = itemsList.Sum(i => i.TotalPrice);
            var uniqueProducts = itemsList.Select(i => i.ProductId).Distinct().Count();

            var topCategories = itemsList
                .GroupBy(i => i.Product?.Category?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            return new OrderItemStatisticsDto
            {
                TotalItemsSold = totalItemsSold,
                TotalRevenue = totalRevenue,
                AverageOrderValue = totalItemsSold > 0 ? totalRevenue / totalItemsSold : 0,
                UniqueProductsSold = uniqueProducts,
                TopCategories = topCategories
            };
        }
        public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount)
        {
            var allItems = await _orderItemRepository.GetAllAsync();

            var productSales = allItems
                .GroupBy(i => new { i.ProductId, i.Product.Name })
                .Select(g => new ProductSalesDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantitySold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.TotalPrice),
                    NumberOfOrders = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(topCount)
                .ToList();

            return productSales;
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

        private OrderItemDto MapToOrderItemDto(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                Id = orderItem.Id,
                Quantity = orderItem.Quantity,
                Price = orderItem.Price,
                TotalPrice = orderItem.TotalPrice,
                OrderId = orderItem.OrderId,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product?.Name ?? "Unknown",
                ProductImage = orderItem.Product?.ProductImages?.FirstOrDefault()?.Url ?? string.Empty
            };
        }
    }
}
