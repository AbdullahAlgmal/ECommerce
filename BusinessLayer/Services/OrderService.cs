using CoreLayer.DTOs;
using CoreLayer.DTOs.Order;
using CoreLayer.DTOs.OrderItem;
using CoreLayer.Enums;
using CoreLayer.Interfaces.Repositories;
using CoreLayer.Interfaces.Services;
using ECommerceApi;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToOrderDto);
        }
        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? MapToOrderDto(order) : null;
        }
        public async Task<OrderDto?> GetOrderWithDetailsAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            return order != null ? MapToOrderDto(order) : null;
        }
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createDto)
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(u => u.Id == createDto.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {createDto.UserId} does not exist");

            if (createDto.Items == null || !createDto.Items.Any())
                throw new InvalidOperationException("Order must contain at least one item");

            var order = new Order
            {
                UserId = createDto.UserId,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Status = (byte)OrderStatus.Pending
            };

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in createDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found");

                if (product.Quantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {itemDto.Quantity}");

                var itemTotal = product.Price * itemDto.Quantity;
                totalAmount += itemTotal;

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = product.Price,
                    TotalPrice = itemTotal,
                    Order = order
                };

                orderItems.Add(orderItem);

                // Reduce product stock
                await _productRepository.ReduceStockAsync(itemDto.ProductId, itemDto.Quantity);
            }

            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;

            var createdOrder = await _orderRepository.AddAsync(order);
            return MapToOrderDto(createdOrder);
        }
        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                return false;

            // Restore product stock if order is cancelled
            if (order.Status != (byte)OrderStatus.Cancelled && order.Status != (byte)OrderStatus.Delivered)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            return await _orderRepository.DeleteAsync(id);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int id, byte newStatus)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found");

            var currentStatus = (OrderStatus)order.Status;
            var targetStatus = (OrderStatus)newStatus;

            if (!currentStatus.CanTransitionTo(targetStatus))
                throw new InvalidOperationException($"Cannot transition from {currentStatus.GetStatusName()} to {targetStatus.GetStatusName()}");

            order.Status = newStatus;
            var updatedOrder = await _orderRepository.UpdateAsync(order);
            return MapToOrderDto(updatedOrder);
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var orders = await _orderRepository.GetOrdersByUserAsync(userId);
            return orders.Select(MapToOrderDto);
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(byte status)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return orders.Select(MapToOrderDto);
        }

        public async Task<PagedResult<OrderDto>> SearchOrdersAsync(OrderFilterDto filter)
        {
            var predicate = BuildPredicate(filter);

            var (orders, totalCount) = await _orderRepository.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                predicate,
                filter.SortBy,
                filter.SortDescending);

            return new PagedResult<OrderDto>
            {
                Items = orders.Select(MapToOrderDto),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            var totalOrders = await _orderRepository.CountAsync();
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
            var statusStats = await _orderRepository.GetOrderStatusStatisticsAsync();
            var currentYear = DateTime.Now.Year;
            var monthlyStats = await _orderRepository.GetMonthlyOrderCountAsync(currentYear);

            return new OrderStatisticsDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                StatusBreakdown = statusStats.ToDictionary(
                    kvp => ((OrderStatus)kvp.Key).GetStatusName(),
                    kvp => kvp.Value),
                MonthlyOrderCounts = monthlyStats
            };
        }
        public async Task<decimal> GetTotalRevenueAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            return await _orderRepository.GetTotalRevenueAsync(fromDate, toDate);
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _orderRepository.ExistsAsync(o => o.Id == id);
        }
        public async Task<bool> OrderBelongsToUserAsync(int orderId, int userId)
        {
            return await _orderRepository.ExistsAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<bool> CanCancelOrderAsync(int orderId)
        {
            return await _orderRepository.ExistsAsync(o =>
                o.Id == orderId &&
                (o.Status == (byte)OrderStatus.Pending || o.Status == (byte)OrderStatus.Processing));
        }

        private Expression<Func<Order, bool>>? BuildPredicate(OrderFilterDto filter)
        {
            Expression<Func<Order, bool>>? predicate = null;

            if (filter.UserId.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.UserId == filter.UserId.Value);
            }

            if (filter.Status.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.Status == filter.Status.Value);
            }

            if (filter.OrderDateFrom.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.OrderDate >= filter.OrderDateFrom.Value);
            }

            if (filter.OrderDateTo.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.OrderDate <= filter.OrderDateTo.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.TotalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                predicate = CombinePredicates(predicate, o => o.TotalAmount <= filter.MaxAmount.Value);
            }

            return predicate;
        }
        private Expression<Func<Order, bool>>? CombinePredicates(
            Expression<Func<Order, bool>>? existing,
            Expression<Func<Order, bool>> newPredicate)
        {
            if (existing == null)
                return newPredicate;

            var parameter = Expression.Parameter(typeof(Order));
            var combined = Expression.AndAlso(
                Expression.Invoke(existing, parameter),
                Expression.Invoke(newPredicate, parameter));

            return Expression.Lambda<Func<Order, bool>>(combined, parameter);
        }
        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusName = ((OrderStatus)order.Status).GetStatusName(),
                UserId = order.UserId,
                UserFullName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}" : "Unknown",
                UserEmail = order.User?.Email ?? "Unknown",
                TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TotalPrice = oi.TotalPrice,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown",
                    ProductImage = oi.Product?.ProductImages?.FirstOrDefault()?.Url ?? string.Empty
                }).ToList() ?? new()
            };
        }
    }
}
