using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.DTOs.Product;
using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataAccessLayer.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {

        public OrderItemRepository(AppDbContext context) : base(context) { }
        public async Task<IEnumerable<OrderItemDto>> GetItemsByOrderAsync(int orderId)
        {

            return await _dbSet
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TotalPrice = oi.TotalPrice,
                    ProductName = oi.Product.Name ?? "Unknown",
                    ProductImage = oi.Product.ProductImages.FirstOrDefault()!.Url ?? "default-image.jpg"
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<OrderItemDto>> GetItemsByProductAsync(int productId)
        {
            return await _dbSet
                .Where(oi => oi.ProductId == productId)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TotalPrice = oi.TotalPrice,
                    ProductName = oi.Product.Name ?? "Unknown",
                    ProductImage = oi.Product.ProductImages.FirstOrDefault()!.Url ?? "default-image.jpg"
                })
                .ToListAsync();
        }
        public async Task<OrderItemStatisticsDto> GetStatisticsAsync()
        {
            FormattableString sql = @$"
                SELECT 
                SUM(oi.Quantity) AS TotalItemsSold,
                SUM(oi.Quantity * oi.Price) AS TotalRevenue,
                CASE 
                    WHEN SUM(oi.Quantity) > 0 
                    THEN SUM(oi.Quantity * oi.Price) / SUM(oi.Quantity)
                    ELSE 0 
                END AS AverageOrderValue,
                COUNT(DISTINCT oi.ProductId) AS UniqueProductsSold,
                (
                    SELECT 
                        ISNULL(c2.Name, 'Unknown') AS Name,
                        SUM(oi2.Quantity) AS Quantity
                    FROM OrderItems oi2
                    JOIN Products p2 ON oi2.ProductId = p2.Id
                    LEFT JOIN Category c2 ON p2.CategoryId = c2.Id
                    GROUP BY c2.Name
                    ORDER BY SUM(oi2.Quantity) DESC
                    FOR JSON PATH
                ) AS TopCategoriesJson
            FROM OrderItems oi
            LEFT JOIN Products p ON oi.ProductId = p.Id
            LEFT JOIN Category c ON p.CategoryId = c.Id";

            var result = await _context.Database
                .SqlQuery<OrderItemStatisticsRaw>(sql)
                .FirstOrDefaultAsync();

            if (result == null)
                return new OrderItemStatisticsDto();

            IEnumerable<CategoryItem> categoryItems = string.IsNullOrEmpty(result.TopCategoriesJson)
                ? new List<CategoryItem>()
                : JsonSerializer.Deserialize<IEnumerable<CategoryItem>>(result.TopCategoriesJson) ?? new List<CategoryItem>();

            return new OrderItemStatisticsDto
            {
                TotalItemsSold = result.TotalItemsSold,
                TotalRevenue = result.TotalRevenue,
                AverageOrderValue = result.AverageOrderValue,
                UniqueProductsSold = result.UniqueProductsSold,
                TopCategories = string.IsNullOrEmpty(result.TopCategoriesJson)
                    ? new Dictionary<string, int>()
                    : categoryItems.ToDictionary(ci => ci.Name, ci => ci.Quantity)
            };
        }
        public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topCount)
        {
            return await _dbSet
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new ProductSalesDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name ?? "Unknown",
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice),
                    NumberOfOrders = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(ps => ps.TotalRevenue)
                .Take(topCount)
                .ToListAsync();
        }
        public async Task<bool> DeleteItemsByOrderAsync(int orderId)
        {
            var items = await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync();
            if (!items.Any())
                return false;

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<decimal> GetTotalSalesByProductAsync(int productId)
        {
            return await _dbSet
                .Where(oi => oi.ProductId == productId)
                .SumAsync(oi => oi.TotalPrice);
        }
    }
}
