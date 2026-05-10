using DataAccessLayer;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IAddressRepository
    {
        Task<Address?> GetByIdAsync(int id);
        Task<Address> AddAsync(Address entity);
        Task<Address> UpdateAsync(Address entity);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<Address>> FindAsync(Expression<Func<Address, bool>> predicate);
        Task<IEnumerable<Address>> GetAddressesByUserAsync(int userId);
        Task<Address?> GetAddressWithUserAsync(int addressId);
        Task<bool> UserOwnsAddressAsync(int addressId, int userId);
        Task<bool> ExistsAsync(Expression<Func<Address, bool>> predicate);

        Task<bool> DeleteAddressesByUserAsync(int userId);
    }
}
