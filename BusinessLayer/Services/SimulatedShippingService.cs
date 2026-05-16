using CoreLayer.DTOs.Shipping;
using CoreLayer.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services
{
    public class SimulatedShippingService : ISimulatedShippingService
    {
        private readonly ILogger<SimulatedShippingService> _logger;
        private static readonly Dictionary<string, List<TrackingEvent>> _shipments = new();
        private static readonly Random _random = new();

        public SimulatedShippingService(ILogger<SimulatedShippingService> logger)
        {
            _logger = logger;
        }

        public async Task<ShippingResult> CreateShipmentAsync(CreateShipmentRequest request)
        {
            var trackingNumber = GenerateTrackingNumber();
            var shippingDate = request.ShippingDate ?? DateOnly.FromDateTime(DateTime.Now);
            var estimatedDays = GetEstimatedDays(request.CarrierName);
            var estimatedDeliveryDate = shippingDate.AddDays(estimatedDays);

            var trackingEvents = new List<TrackingEvent>
            {
                new()
                {
                    Date = DateTime.Now,
                    Status = "Label Created",
                    Location = "Warehouse",
                    Description = "Shipping label has been created"
                },
                new()
                {
                    Date = DateTime.Now.AddHours(2),
                    Status = "Picked Up",
                    Location = "Warehouse",
                    Description = "Shipment has been picked up by carrier"
                }
            };

            _shipments[trackingNumber] = trackingEvents;

            _logger.LogInformation($"Shipment created for Order {request.OrderId}. Tracking: {trackingNumber}");

            return new ShippingResult
            {
                IsSuccess = true,
                TrackingNumber = trackingNumber,
                CarrierName = request.CarrierName,
                EstimatedDeliveryDate = estimatedDeliveryDate,
                Status = "Processing",
                Message = "Shipment created successfully"
            };
        }
        public async Task<ShippingTrackingResult> TrackShipmentAsync(string trackingNumber)
        {
            if (!_shipments.ContainsKey(trackingNumber))
            {
                return new ShippingTrackingResult
                {
                    IsSuccess = false,
                    TrackingNumber = trackingNumber,
                    Status = "NotFound",
                    TrackingHistory = new List<TrackingEvent>()
                };
            }

            var events = _shipments[trackingNumber];

            await SimulateProgress(trackingNumber);

            return new ShippingTrackingResult
            {
                IsSuccess = true,
                TrackingNumber = trackingNumber,
                Status = events.Last().Status,
                TrackingHistory = events
            };
        }
        public async Task<bool> CancelShipmentAsync(string trackingNumber)
        {
            if (_shipments.ContainsKey(trackingNumber))
            {
                var events = _shipments[trackingNumber];
                if (events.Last().Status != "Delivered")
                {
                    events.Add(new TrackingEvent
                    {
                        Date = DateTime.Now,
                        Status = "Cancelled",
                        Location = "System",
                        Description = "Shipment has been cancelled"
                    });

                    _logger.LogInformation($"Shipment {trackingNumber} cancelled");
                    return true;
                }
            }

            return false;
        }
        public async Task<ShippingRateResult> CalculateShippingRateAsync(ShippingRateRequest request)
        {
            var baseRate = 5.99m;
            var weightCharge = request.TotalWeight * 0.50m;
            var itemCharge = request.TotalItems * 0.25m;
            var totalRate = baseRate + weightCharge + itemCharge;

            var carriers = new[] { "Standard Post", "Express Delivery", "FastShip Logistics" };
            var randomCarrier = carriers[_random.Next(carriers.Length)];
            var estimatedDays = _random.Next(2, 8);

            return new ShippingRateResult
            {
                CarrierName = randomCarrier,
                Rate = Math.Round(totalRate, 2),
                EstimatedDays = estimatedDays
            };
        }

        private string GenerateTrackingNumber()
        {
            return $"TRK_{DateTime.Now:yyyyMMdd}_{_random.Next(100000, 999999)}";
        }
        private int GetEstimatedDays(string carrierName)
        {
            return carrierName.ToLower() switch
            {
                var c when c.Contains("express") => 2,
                var c when c.Contains("fast") => 3,
                _ => 7
            };
        }
        private async Task SimulateProgress(string trackingNumber)
        {
            var events = _shipments[trackingNumber];
            var lastEvent = events.Last();

            if (lastEvent.Status == "Picked Up" && DateTime.Now > lastEvent.Date.AddHours(12))
            {
                events.Add(new TrackingEvent
                {
                    Date = DateTime.Now,
                    Status = "In Transit",
                    Location = "Sorting Facility",
                    Description = "Shipment is in transit to destination"
                });
            }
            else if (lastEvent.Status == "In Transit" && DateTime.Now > lastEvent.Date.AddHours(24))
            {
                events.Add(new TrackingEvent
                {
                    Date = DateTime.Now,
                    Status = "Out for Delivery",
                    Location = "Local Delivery Center",
                    Description = "Shipment is out for delivery"
                });
            }
            else if (lastEvent.Status == "Out for Delivery" && DateTime.Now > lastEvent.Date.AddHours(8))
            {
                events.Add(new TrackingEvent
                {
                    Date = DateTime.Now,
                    Status = "Delivered",
                    Location = "Customer Address",
                    Description = "Shipment has been delivered"
                });
            }
        }
    }
}
