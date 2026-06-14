using BusinessLayer.DTOs.Order;
using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace DataAccessLayer.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<OrderDto?> GetOrderWithDetailsAsync(int id)
        {
            var sql = @"
                SELECT 
                o.Id AS OrderId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                CASE 
                    WHEN o.Status = 1 THEN 'Pending'
                    WHEN o.Status = 2 THEN 'Processing'
                    WHEN o.Status = 3 THEN 'Shipped'
                    WHEN o.Status = 4 THEN 'Delivered'
                    WHEN o.Status = 5 THEN 'Cancelled'
		            WHEN o.Status = 6 THEN 'Refunded'
                END AS StatusName,
                o.UserId,
                u.FirstName + u.LastName AS UserFullName,
                u.Email AS UserEmail,
                COUNT(DISTINCT oi.Id) AS TotalItems,
                (
                    SELECT 
                        oi2.Id,
                        oi2.Quantity,
                        oi2.Price,
                        oi2.Quantity * oi2.Price AS TotalPrice,
                        oi2.OrderId,
                        p2.Id AS ProductId,
                        p2.Name AS ProductName,
                        COALESCE(pi2.Url, '') AS ProductImage
                    FROM OrderItems oi2
                    INNER JOIN Products p2 ON oi2.ProductId = p2.Id
                    LEFT JOIN ProductImages pi2 ON p2.Id = pi2.ProductId AND pi2.ImageOrder = 1
                    WHERE oi2.OrderId = o.Id
                    FOR JSON PATH
                ) AS OrderItemsJson
            FROM Orders o
            INNER JOIN Users u ON o.UserId = u.Id
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            WHERE o.Id = @orderId
            GROUP BY o.Id, o.OrderDate, o.TotalAmount, o.Status, o.UserId, u.FirstName + u.LastName, u.Email";

            var result = await _context.Set<OrderAggregatedRaw>()
                .FromSqlRaw(sql, new SqlParameter("@orderId", id)).FirstOrDefaultAsync();

            // Map to DTOs
            var orderDto = new OrderDto
            {
                Id = result?.OrderId ?? 0,
                OrderDate = result?.OrderDate ?? DateOnly.MinValue,
                TotalAmount = result?.TotalAmount ?? 0,
                Status = result?.Status ?? 0,
                StatusName = result?.StatusName ?? string.Empty,
                UserId = result?.UserId ?? 0,
                UserFullName = result?.UserFullName ?? string.Empty,
                UserEmail = result?.UserEmail ?? string.Empty,
                TotalItems = result?.TotalItems ?? 0,
                OrderItems = string.IsNullOrEmpty(result?.OrderItemsJson)
                    ? new List<OrderItemDto>()
                    : JsonSerializer.Deserialize<List<OrderItemDto>>(result.OrderItemsJson)!
            };

            return orderDto;
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var sql = @"
                SELECT 
                o.Id AS OrderId,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                CASE 
                    WHEN o.Status = 1 THEN 'Pending'
                    WHEN o.Status = 2 THEN 'Processing'
                    WHEN o.Status = 3 THEN 'Shipped'
                    WHEN o.Status = 4 THEN 'Delivered'
                    WHEN o.Status = 5 THEN 'Cancelled'
		            WHEN o.Status = 6 THEN 'Refunded'
                END AS StatusName,
                o.UserId,
                u.FirstName + u.LastName AS UserFullName,
                u.Email AS UserEmail,
                COUNT(DISTINCT oi.Id) AS TotalItems,
                (
                    SELECT 
                        oi2.Id,
                        oi2.Quantity,
                        oi2.Price,
                        oi2.Quantity * oi2.Price AS TotalPrice,
                        oi2.OrderId,
                        p2.Id AS ProductId,
                        p2.Name AS ProductName,
                        COALESCE(pi2.Url, '') AS ProductImage
                    FROM OrderItems oi2
                    INNER JOIN Products p2 ON oi2.ProductId = p2.Id
                    LEFT JOIN ProductImages pi2 ON p2.Id = pi2.ProductId AND pi2.ImageOrder = 1
                    WHERE oi2.OrderId = o.Id
                    FOR JSON PATH
                ) AS OrderItemsJson
            FROM Orders o
            INNER JOIN Users u ON o.UserId = u.Id
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            WHERE o.UserId = @userId
            GROUP BY o.Id, o.OrderDate, o.TotalAmount, o.Status, o.UserId, u.FirstName + u.LastName, u.Email
            order by o.OrderDate DESC";

            var results = await _context.Set<OrderAggregatedRaw>()
                .FromSqlRaw(sql, new SqlParameter("@userId", userId))
                .ToListAsync();

            // Map to DTOs
            var ordersDto = results.Select(r => new OrderDto
            {
                Id = r.OrderId,
                OrderDate = r.OrderDate,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                StatusName = r.StatusName,
                UserId = r.UserId,
                UserFullName = r.UserFullName,
                UserEmail = r.UserEmail,
                TotalItems = r.TotalItems,
                OrderItems = string.IsNullOrEmpty(r.OrderItemsJson)
                    ? new List<OrderItemDto>()
                    : JsonSerializer.Deserialize<List<OrderItemDto>>(r.OrderItemsJson)!
            }).ToList();

            return ordersDto;
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(byte status)
        {
            var sql = @"
                    SELECT 
                    o.Id AS OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    CASE 
                        WHEN o.Status = 1 THEN 'Pending'
                        WHEN o.Status = 2 THEN 'Processing'
                        WHEN o.Status = 3 THEN 'Shipped'
                        WHEN o.Status = 4 THEN 'Delivered'
                        WHEN o.Status = 5 THEN 'Cancelled'
		                WHEN o.Status = 6 THEN 'Refunded'
                    END AS StatusName,
                    o.UserId,
                    u.FirstName + u.LastName AS UserFullName,
                    u.Email AS UserEmail,
                    COUNT(DISTINCT oi.Id) AS TotalItems,
                    (
                        SELECT 
                            oi2.Id,
                            oi2.Quantity,
                            oi2.Price,
                            oi2.Quantity * oi2.Price AS TotalPrice,
                            oi2.OrderId,
                            p2.Id AS ProductId,
                            p2.Name AS ProductName,
                            COALESCE(pi2.Url, '') AS ProductImage
                        FROM OrderItems oi2
                        INNER JOIN Products p2 ON oi2.ProductId = p2.Id
                        LEFT JOIN ProductImages pi2 ON p2.Id = pi2.ProductId AND pi2.ImageOrder = 1
                        WHERE oi2.OrderId = o.Id
                        FOR JSON PATH
                    ) AS OrderItemsJson
                FROM Orders o
                INNER JOIN Users u ON o.UserId = u.Id
                LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                WHERE o.Status = @status
                GROUP BY o.Id, o.OrderDate, o.TotalAmount, o.Status, o.UserId, u.FirstName + u.LastName, u.Email
                order by o.OrderDate DESC";

            var results = await _context.Set<OrderAggregatedRaw>()
                .FromSqlRaw(sql, new SqlParameter("@status", status))
                .ToListAsync();

            // Map to DTOs
            var ordersDto = results.Select(r => new OrderDto
            {
                Id = r.OrderId,
                OrderDate = r.OrderDate,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                StatusName = r.StatusName,
                UserId = r.UserId,
                UserFullName = r.UserFullName,
                UserEmail = r.UserEmail,
                TotalItems = r.TotalItems,
                OrderItems = string.IsNullOrEmpty(r.OrderItemsJson)
                    ? new List<OrderItemDto>()
                    : JsonSerializer.Deserialize<List<OrderItemDto>>(r.OrderItemsJson)!
            }).ToList();

            return ordersDto;
        }
        public async Task<IEnumerable<OrderDto>> GetOrdersByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var sql = @"
                    SELECT 
                    o.Id AS OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    CASE 
                        WHEN o.Status = 1 THEN 'Pending'
                        WHEN o.Status = 2 THEN 'Processing'
                        WHEN o.Status = 3 THEN 'Shipped'
                        WHEN o.Status = 4 THEN 'Delivered'
                        WHEN o.Status = 5 THEN 'Cancelled'
		                WHEN o.Status = 6 THEN 'Refunded'
                    END AS StatusName,
                    o.UserId,
                    u.FirstName + u.LastName AS UserFullName,
                    u.Email AS UserEmail,
                    COUNT(DISTINCT oi.Id) AS TotalItems,
                    (
                        SELECT 
                            oi2.Id,
                            oi2.Quantity,
                            oi2.Price,
                            oi2.Quantity * oi2.Price AS TotalPrice,
                            oi2.OrderId,
                            p2.Id AS ProductId,
                            p2.Name AS ProductName,
                            COALESCE(pi2.Url, '') AS ProductImage
                        FROM OrderItems oi2
                        INNER JOIN Products p2 ON oi2.ProductId = p2.Id
                        LEFT JOIN ProductImages pi2 ON p2.Id = pi2.ProductId AND pi2.ImageOrder = 1
                        WHERE oi2.OrderId = o.Id
                        FOR JSON PATH
                    ) AS OrderItemsJson
                FROM Orders o
                INNER JOIN Users u ON o.UserId = u.Id
                LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                WHERE o.OrderDate between @startDate and @endDate
                GROUP BY o.Id, o.OrderDate, o.TotalAmount, o.Status, o.UserId, u.FirstName + u.LastName, u.Email
                order by o.OrderDate DESC";

            var results = await _context.Set<OrderAggregatedRaw>()
                .FromSqlRaw(sql, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate))
                .ToListAsync();

            // Map to DTOs
            var ordersDto = results.Select(r => new OrderDto
            {
                Id = r.OrderId,
                OrderDate = r.OrderDate,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                StatusName = r.StatusName,
                UserId = r.UserId,
                UserFullName = r.UserFullName,
                UserEmail = r.UserEmail,
                TotalItems = r.TotalItems,
                OrderItems = string.IsNullOrEmpty(r.OrderItemsJson)
                    ? new List<OrderItemDto>()
                    : JsonSerializer.Deserialize<List<OrderItemDto>>(r.OrderItemsJson)!
            }).ToList();

            return ordersDto;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _dbSet.Where(o => o.Status != (byte)CoreLayer.Enums.OrderStatus.Cancelled);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }
        public async Task<Dictionary<byte, int>> GetOrderStatusStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }
        public async Task<Dictionary<int, int>> GetMonthlyOrderCountAsync(int year)
        {
            return await _dbSet
                .Where(o => o.OrderDate.Year == year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Month, g => g.Count);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Order, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = _dbSet
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDescending);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        private IQueryable<Order> ApplySorting(IQueryable<Order> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return sortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);

            return sortBy.ToLower() switch
            {
                "orderdate" => sortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
                "totalamount" => sortDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "status" => sortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => sortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate)
            };
        }
    }
    
}
