using BusinessLayer.DTOs.Payment;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISimulatedPaymentService _simulatedPaymentService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ISimulatedPaymentService simulatedPaymentService)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _simulatedPaymentService = simulatedPaymentService;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return payment != null ? MapToPaymentDto(payment) : null;
        }
        public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createDto)
        {
            var orderExists = await _orderRepository.ExistsAsync(o => o.Id == createDto.OrderId);
            if (!orderExists)
                throw new InvalidOperationException($"Order with ID {createDto.OrderId} does not exist");

            var userExists = await _userRepository.ExistsAsync(u => u.Id == createDto.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {createDto.UserId} does not exist");

            var existingPayment = await _paymentRepository.GetPaymentByOrderAsync(createDto.OrderId);
            if (existingPayment != null)
                throw new InvalidOperationException($"Payment already exists for order {createDto.OrderId}");

            var paymentRequest = new PaymentRequest
            {
                OrderId = createDto.OrderId,
                UserId = createDto.UserId,
                Amount = createDto.Amount,
                PaymentMethod = createDto.Method,
                CardNumber = createDto.CardNumber,
                ExpiryDate = createDto.ExpiryDate,
                Cvv = createDto.Cvv,
                CardHolderName = createDto.CardHolderName
            };

            var result = await _simulatedPaymentService.ProcessPaymentAsync(paymentRequest);

            var payment = new Payment
            {
                OrderId = createDto.OrderId,
                UserId = createDto.UserId,
                Amount = createDto.Amount,
                Method = createDto.Method,
                PaymentDate = DateOnly.FromDateTime(DateTime.Today),
                Status = result.Status,
                TransactionId = result.TransactionId
            };

            var createdPayment = await _paymentRepository.AddAsync(payment);

            if (result.IsSuccess)
            {
                var order = await _orderRepository.GetByIdAsync(createDto.OrderId);
                if (order != null)
                {
                    order.Status = 2;
                    await _orderRepository.UpdateAsync(order);
                }
            }

            return MapToPaymentDto(createdPayment);
        }
        public async Task<PaymentDto> RefundPaymentAsync(RefundPaymentDto refundDto)
        {
            var payment = await _paymentRepository.GetByIdAsync(refundDto.PaymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {refundDto.PaymentId} not found");

            if (payment.Status != "Completed")
                throw new InvalidOperationException("Only completed payments can be refunded");

            var result = await _simulatedPaymentService.RefundPaymentAsync(refundDto.PaymentId, refundDto.Amount);

            if (result.IsSuccess)
            {
                payment.Status = "Refunded";
                await _paymentRepository.UpdateAsync(payment);

                var order = await _orderRepository.GetByIdAsync(payment.OrderId);
                if (order != null)
                {
                    order.Status = 6;
                    await _orderRepository.UpdateAsync(order);
                }
            }

            return MapToPaymentDto(payment);
        } 
        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserAsync(int userId)
        {
            var payments = await _paymentRepository.GetPaymentsByUserAsync(userId);
            return payments;
        }
        public async Task<PaymentDto?> GetPaymentByOrderAsync(int orderId)
        {
            var payment = await _paymentRepository.GetPaymentByOrderAsync(orderId);
            return payment;
        }
        public async Task<decimal> GetTotalUserSpendingAsync(int userId)
        {
            return await _paymentRepository.GetTotalPaymentsByUserAsync(userId);
        }

        private PaymentDto MapToPaymentDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                Amount = payment.Amount,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                UserName = payment.User != null ? $"{payment.User.FirstName} {payment.User.LastName}" : "Unknown",
                Status = payment.Status,
                TransactionId = payment.TransactionId
            };
        }
    }
}
