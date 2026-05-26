using BusinessLayer.DTOs.Shipping;
using CoreLayer.Enums;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IShippingRepository _shippingRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ISimulatedShippingService _simulatedShippingService;

        public ShippingService(
            IShippingRepository shippingRepository,
            IOrderRepository orderRepository,
            IAddressRepository addressRepository,
            ISimulatedShippingService simulatedShippingService)
        {
            _shippingRepository = shippingRepository;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
            _simulatedShippingService = simulatedShippingService;
        }

        public async Task<IEnumerable<ShippingDto>> GetAllShippingsAsync()
        {
            var shippings = await _shippingRepository.GetAllAsync();
            var shippingDtos = new List<ShippingDto>();
            foreach (var shipping in shippings)
            {
                shippingDtos.Add(await MapToShippingDto(shipping));
            }
            return shippingDtos;
        }
        public async Task<ShippingDto?> GetShippingByIdAsync(int id)
        {
            var shipping = await _shippingRepository.GetByIdAsync(id);
            return shipping != null ? await MapToShippingDto(shipping) : null;
        }
        public async Task<ShippingDto> CreateShippingAsync(CreateShippingDto createDto)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == createDto.OrderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {createDto.OrderId} does not exist");

            var addressExists = await _addressRepository.ExistsAsync(a => a.Id == createDto.AddressId);
            if (!addressExists)
                throw new InvalidOperationException($"Address with ID {createDto.AddressId} does not exist");

            var existingShipping = await _shippingRepository.GetShippingByOrderAsync(createDto.OrderId);
            if (existingShipping != null)
                throw new InvalidOperationException($"Shipping already exists for order {createDto.OrderId}");

            var shippingRequest = new CreateShipmentRequest
            {
                OrderId = createDto.OrderId,
                AddressId = createDto.AddressId,
                CarrierName = createDto.CarrierName,
                ShippingDate = createDto.ShippingDate
            };

            var result = await _simulatedShippingService.CreateShipmentAsync(shippingRequest);

            var shipping = new Shipping
            {
                OrderId = createDto.OrderId,
                AddressId = createDto.AddressId,
                CarrierName = createDto.CarrierName,
                TrackingNumber = result.TrackingNumber,
                ShippingDate = createDto.ShippingDate ?? DateOnly.FromDateTime(DateTime.Now),
                DeliveryDate = result.EstimatedDeliveryDate,
                Status = (byte)ShippingStatus.Processing
            };

            var createdShipping = await _shippingRepository.AddAsync(shipping);

            var order = await _orderRepository.GetByIdAsync(createDto.OrderId);
            if (order != null && order.Status == 2)
            {
                order.Status = 3;
                await _orderRepository.UpdateAsync(order);
            }

            return await MapToShippingDto(createdShipping);
        }
        public async Task<ShippingDto> UpdateShippingStatusAsync(int id, UpdateShippingStatusDto updateDto)
        {
            var shipping = await _shippingRepository.GetByIdAsync(id);
            if (shipping == null)
                throw new KeyNotFoundException($"Shipping with ID {id} not found");

            var oldStatus = (ShippingStatus)shipping.Status;
            var newStatus = (ShippingStatus)updateDto.Status;

            shipping.Status = updateDto.Status;

            if (updateDto.DeliveryDate.HasValue)
                shipping.DeliveryDate = updateDto.DeliveryDate.Value;

            var updatedShipping = await _shippingRepository.UpdateAsync(shipping);
           
            if (newStatus == ShippingStatus.Delivered && oldStatus != ShippingStatus.Delivered)
            {
                var order = await _orderRepository.GetByIdAsync(shipping.OrderId);
                if (order != null)
                {
                    order.Status = 4; 
                    await _orderRepository.UpdateAsync(order);
                }
            }

            return await MapToShippingDto(updatedShipping);
        }
        public async Task<bool> CancelShippingAsync(int id)
        {
            var shipping = await _shippingRepository.GetByIdAsync(id);
            if (shipping == null)
                return false;

            var result = await _simulatedShippingService.CancelShipmentAsync(shipping.TrackingNumber);

            if (result)
            {
                shipping.Status = (byte)ShippingStatus.Cancelled;
                await _shippingRepository.UpdateAsync(shipping);
            }

            return result;
        }
        public async Task<ShippingDto?> GetShippingByOrderAsync(int orderId)
        {
            var shipping = await _shippingRepository.GetShippingByOrderAsync(orderId);
            return shipping != null ? await MapToShippingDto(shipping) : null;
        }
        public async Task<ShippingTrackingResult> TrackShipmentAsync(string trackingNumber)
        {
            return await _simulatedShippingService.TrackShipmentAsync(trackingNumber);
        }
        public async Task<ShippingRateResult> CalculateShippingRateAsync(int addressId, decimal totalWeight, int totalItems)
        {
            var addressExists = await _addressRepository.ExistsAsync(a => a.Id == addressId);
            if (!addressExists)
                throw new InvalidOperationException($"Address with ID {addressId} does not exist");

            var request = new ShippingRateRequest
            {
                AddressId = addressId,
                TotalWeight = totalWeight,
                TotalItems = totalItems
            };

            return await _simulatedShippingService.CalculateShippingRateAsync(request);
        }

        private async Task<List<ShippingTrackingEventDto>> GetTrackingHistoryAsync(string trackingNumber)
        {
            var tracking = await _simulatedShippingService.TrackShipmentAsync(trackingNumber);
            return tracking.TrackingHistory.Select(e => new ShippingTrackingEventDto
            {
                Date = e.Date,
                Status = e.Status,
                Location = e.Location,
                Description = e.Description
            }).ToList();
        }
        private async Task<ShippingDto> MapToShippingDto(Shipping shipping)
        {
            var trackingHistory = await GetTrackingHistoryAsync(shipping.TrackingNumber);
            var address = await _addressRepository.GetByIdAsync(shipping.AddressId);

            return new ShippingDto
            {
                Id = shipping.Id,
                ShippingDate = shipping.ShippingDate,
                DeliveryDate = shipping.DeliveryDate,
                Status = shipping.Status,
                StatusName = ((ShippingStatus)shipping.Status).GetStatusName(),
                CarrierName = shipping.CarrierName,
                TrackingNumber = shipping.TrackingNumber,
                AddressId = shipping.AddressId,
                FullAddress = address != null ? $"{address.HouseNumber}, {address.StreetBlock}, {address.City}" : "Unknown",
                OrderId = shipping.OrderId,
                TrackingHistory = trackingHistory
            };
        }
    }
}
