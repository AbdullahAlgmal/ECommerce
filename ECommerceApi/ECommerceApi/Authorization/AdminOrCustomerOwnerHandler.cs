using CoreLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerceApi.Authorization
{
    public class AdminOrCustomerOwnerHandler : AuthorizationHandler<AdminOrCustomerOwnerRequirement, int>
    {
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IReviewService _reviewService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;

        public AdminOrCustomerOwnerHandler(
            IOrderService orderService,
            IAddressService addressService,
            IReviewService reviewService,
            IPaymentService paymentService,
            IShippingService shippingService)
        {
            _orderService = orderService;
            _addressService = addressService;
            _reviewService = reviewService;
            _paymentService = paymentService;
            _shippingService = shippingService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminOrCustomerOwnerRequirement requirement,
            int resourceId)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return;
            }

            var isOwner = requirement.ResourceType switch
            {
                "User" => currentUserId == resourceId,
                "Order" => await _orderService.OrderBelongsToUserAsync(resourceId, currentUserId),
                "Address" => await _addressService.AddressBelongsToUserAsync(resourceId, currentUserId),
                "Review" => await CheckReviewOwnership(resourceId, currentUserId),
                "Payment" => await CheckPaymentOwnership(resourceId, currentUserId),
                "Shipping" => await CheckShippingOwnership(resourceId, currentUserId),
                _ => false
            };

            if (isOwner)
            {
                context.Succeed(requirement);
            }
        }

        private async Task<bool> CheckReviewOwnership(int reviewId, int userId)
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            return review != null && review.UserId == userId;
        }
        private async Task<bool> CheckPaymentOwnership(int paymentId, int userId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            return payment != null && payment.UserId == userId;
        }
        private async Task<bool> CheckShippingOwnership(int shippingId, int userId)
        {
            var shipping = await _shippingService.GetShippingByIdAsync(shippingId);
            if (shipping == null) return false;

            var order = await _orderService.GetOrderByIdAsync(shipping.OrderId);
            return order != null && order.UserId == userId;
        }
    }
}
