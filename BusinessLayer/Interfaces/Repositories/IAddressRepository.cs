
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        Task<Address?> GetAddressByUserAsync(int userId);
        Task<Address?> GetAddressWithUserAsync(int addressId);
        Task<bool> UserOwnsAddressAsync(int addressId, int userId);
        Task<bool> DeleteAddressesByUserAsync(int userId);
    }
}
