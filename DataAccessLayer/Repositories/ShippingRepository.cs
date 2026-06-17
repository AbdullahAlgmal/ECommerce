using BusinessLayer.DTOs.Shipping;
using BusinessLayer.Interfaces.Repositories;
using CoreLayer.Enums;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ShippingRepository : BaseRepository<Shipping>, IShippingRepository
    {
        public ShippingRepository(AppDbContext context) : base(context) { }

        public new async Task<IEnumerable<ShippingDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    ShippingDate = s.ShippingDate,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CarrierName = s.CarrierName,
                    TrackingNumber = s.TrackingNumber,
                    AddressId = s.AddressId,
                    FullAddress = $@"
Country: {s.Address.Country},
Street: {s.Address.StreetBlock},
Area: {s.Address.Area},
House Number: {s.Address.HouseNumber},
City: {s.Address.City},
State: {s.Address.Province},
Zip Code: {s.Address.ZipCode}",
                    OrderId = s.OrderId
                })
                .OrderByDescending(s => s.ShippingDate)
                .ToListAsync();
        }
        public new async Task<ShippingDto?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(s => s.Id == id)
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    ShippingDate = s.ShippingDate,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CarrierName = s.CarrierName,
                    TrackingNumber = s.TrackingNumber,
                    AddressId = s.AddressId,
                    FullAddress = $@"
Country: {s.Address.Country},
Street: {s.Address.StreetBlock},
Area: {s.Address.Area},
House Number: {s.Address.HouseNumber},
City: {s.Address.City},
State: {s.Address.Province},
Zip Code: {s.Address.ZipCode}",
                    OrderId = s.OrderId
                }).FirstOrDefaultAsync();
        }
        public async Task<ShippingDto?> GetShippingByOrderAsync(int orderId)
        {
            return await _dbSet
                .Where(s => s.OrderId == orderId)
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    ShippingDate = s.ShippingDate,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CarrierName = s.CarrierName,
                    TrackingNumber = s.TrackingNumber,
                    AddressId = s.AddressId,
                    FullAddress = $@"
Country: {s.Address.Country},
Street: {s.Address.StreetBlock},
Area: {s.Address.Area},
House Number: {s.Address.HouseNumber},
City: {s.Address.City},
State: {s.Address.Province},
Zip Code: {s.Address.ZipCode}",
                    OrderId = s.OrderId
                }).FirstOrDefaultAsync();
        }
        public async Task<ShippingDto?> GetShippingByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbSet
                .Where(s => s.TrackingNumber == trackingNumber)
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    ShippingDate = s.ShippingDate,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CarrierName = s.CarrierName,
                    TrackingNumber = s.TrackingNumber,
                    AddressId = s.AddressId,
                    FullAddress = $@"
Country: {s.Address.Country},
Street: {s.Address.StreetBlock},
Area: {s.Address.Area},
House Number: {s.Address.HouseNumber},
City: {s.Address.City},
State: {s.Address.Province},
Zip Code: {s.Address.ZipCode}",
                    OrderId = s.OrderId
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<ShippingDto>> GetShippingsByAddressAsync(int addressId)
        {
            return await _dbSet
                .Where(s => s.AddressId == addressId)
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    ShippingDate = s.ShippingDate,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CarrierName = s.CarrierName,
                    TrackingNumber = s.TrackingNumber,
                    AddressId = s.AddressId,
                    FullAddress = $@"
Country: {s.Address.Country},
Street: {s.Address.StreetBlock},
Area: {s.Address.Area},
House Number: {s.Address.HouseNumber},
City: {s.Address.City},
State: {s.Address.Province},
Zip Code: {s.Address.ZipCode}",
                    OrderId = s.OrderId
                })
                .OrderByDescending(s => s.ShippingDate)
                .ToListAsync();
        }
    }
}
