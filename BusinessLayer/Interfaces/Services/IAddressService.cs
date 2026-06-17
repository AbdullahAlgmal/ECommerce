using BusinessLayer.DTOs.Address;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IAddressService : IBaseService<Address, AddressDto, CreateAddressDto, UpdateAddressDto>
    {
        Task<AddressDto?> GetAddressByUserAsync(int userId);
        Task<bool> AddressBelongsToUserAsync(int addressId, int userId);
    }
}
