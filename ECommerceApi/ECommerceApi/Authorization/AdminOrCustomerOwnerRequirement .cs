using Microsoft.AspNetCore.Authorization;

namespace ECommerceApi.Authorization
{
    public class AdminOrCustomerOwnerRequirement : IAuthorizationRequirement
    {
        public string ResourceType { get; }

        public AdminOrCustomerOwnerRequirement(string resourceType)
        {
            ResourceType = resourceType;
        }
    }
}
