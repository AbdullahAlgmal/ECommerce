using BusinessLayer.DTOs.Address;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;

        public AddressService(IAddressRepository addressRepository, IUserRepository userRepository)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
        }

        public async Task<AddressDto?> GetAddressByIdAsync(int id)
        {
            var address = await _addressRepository.GetAddressWithUserAsync(id);
            return address != null ? await MapToAddressDto(address) : null;
        }
        public async Task<IEnumerable<AddressDto>> GetAddressesByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var addresses = await _addressRepository.GetAddressesByUserAsync(userId);
            var addressDtos = new List<AddressDto>();

            foreach (var address in addresses)
            {
                addressDtos.Add(await MapToAddressDto(address));
            }

            return addressDtos;
        }
        public async Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == createAddressDto.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {createAddressDto.UserId} does not exist");

            var address = new Address
            {
                HouseNumber = createAddressDto.HouseNumber,
                StreetBlock = createAddressDto.StreetBlock,
                Area = createAddressDto.Area,
                City = createAddressDto.City,
                Province = createAddressDto.Province,
                Country = createAddressDto.Country,
                ZipCode = createAddressDto.ZipCode,
                UserId = createAddressDto.UserId
            };

            var createdAddress = await _addressRepository.AddAsync(address);
            return await MapToAddressDto(createdAddress);
        }
        public async Task<AddressDto> UpdateAddressAsync(int id, UpdateAddressDto updateAddressDto)
        {
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
                throw new KeyNotFoundException($"Address with ID {id} not found");

            address.HouseNumber = updateAddressDto.HouseNumber;
            address.StreetBlock = updateAddressDto.StreetBlock;
            address.Area = updateAddressDto.Area;
            address.City = updateAddressDto.City;
            address.Province = updateAddressDto.Province;
            address.Country = updateAddressDto.Country;
            address.ZipCode = updateAddressDto.ZipCode;

            var updatedAddress = await _addressRepository.UpdateAsync(address);
            return await MapToAddressDto(updatedAddress);
        }
        public async Task<bool> DeleteAddressAsync(int id)
        {
            return await _addressRepository.DeleteAsync(id);
        }

        public async Task<bool> AddressBelongsToUserAsync(int addressId, int userId)
        {
            return await _addressRepository.UserOwnsAddressAsync(addressId, userId);
        }

        private async Task<AddressDto> MapToAddressDto(Address address)
        {
            var user = await _userRepository.GetByIdAsync(address.UserId);

            return new AddressDto
            {
                Id = address.Id,
                HouseNumber = address.HouseNumber,
                StreetBlock = address.StreetBlock,
                Area = address.Area,
                City = address.City,
                Province = address.Province,
                Country = address.Country,
                ZipCode = address.ZipCode,
                UserId = address.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown"
            };
        }
    }
}
