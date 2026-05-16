using CoreLayer.DTOs.Payment;
using CoreLayer.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services
{
    public class SimulatedPaymentService : ISimulatedPaymentService
    {
        private readonly ILogger<SimulatedPaymentService> _logger;
        private static readonly Random _random = new();

        public SimulatedPaymentService(ILogger<SimulatedPaymentService> logger)
        {
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {

            if (!ValidatePaymentDetails(request))
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    TransactionId = GenerateTransactionId(),
                    Status = "Failed",
                    Message = "Invalid payment details. Please check your information.",
                    TransactionDate = DateTime.UtcNow
                };
            }

            var isSuccess = _random.Next(1, 100) <= 95;

            var transactionId = GenerateTransactionId();

            _logger.LogInformation($"Payment processed for Order {request.OrderId}. Transaction: {transactionId}. Success: {isSuccess}");

            return new PaymentResult
            {
                IsSuccess = isSuccess,
                TransactionId = transactionId,
                Status = isSuccess ? "Completed" : "Failed",
                Message = isSuccess ? "Payment completed successfully" : "Payment failed. Please try again.",
                TransactionDate = DateTime.UtcNow
            };
        }
        public async Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal amount)
        {
            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = GenerateTransactionId(),
                Status = "Refunded",
                Message = $"Refund of {amount:C} processed successfully",
                TransactionDate = DateTime.UtcNow
            };
        }

        private bool ValidatePaymentDetails(PaymentRequest request)
        {
            if (request.PaymentMethod == "CreditCard")
            {
                return !string.IsNullOrEmpty(request.CardNumber) &&
                       request.CardNumber.Length >= 13 &&
                       request.CardNumber.Length <= 19 &&
                       !string.IsNullOrEmpty(request.ExpiryDate) &&
                       !string.IsNullOrEmpty(request.Cvv) &&
                       request.Cvv.Length >= 3 &&
                       !string.IsNullOrEmpty(request.CardHolderName);
            }

            return true;
        }
        private string GenerateTransactionId()
        {
            return $"TXN_{DateTime.Now:yyyyMMddHHmmss}_{_random.Next(100000, 999999)}";
        }
    }
}
