using BusinessLayer.DTOs.Address;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IAddressService : IBaseService<Address, AddressDto, CreateAddressDto, UpdateAddressDto>
    {
        Task<IEnumerable<AddressDto>> GetAddressesByUserAsync(int userId);
        Task<bool> AddressBelongsToUserAsync(int addressId, int userId);
    }
}
