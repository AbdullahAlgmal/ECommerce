using BusinessLayer.DTOs.Address;

namespace BusinessLayer.Interfaces.Services
{
    public interface IAddressService
    {
        Task<AddressDto?> GetAddressByIdAsync(int id);
        Task<IEnumerable<AddressDto>> GetAddressesByUserAsync(int userId);
        Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto);
        Task<AddressDto> UpdateAddressAsync(int id, UpdateAddressDto updateAddressDto);
        Task<bool> DeleteAddressAsync(int id);
        Task<bool> AddressBelongsToUserAsync(int addressId, int userId);
    }
}
