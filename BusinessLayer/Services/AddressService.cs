using BusinessLayer.DTOs.Address;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class AddressService : BaseService<Address, AddressDto, CreateAddressDto, UpdateAddressDto>, IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;

        public AddressService(IAddressRepository addressRepository, IUserRepository userRepository) : base(addressRepository)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
        }

        public async Task<AddressDto?> GetAddressByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var address = await _addressRepository.GetAddressByUserAsync(userId);

            return address != null ? await MapToDto(address) : null;
        }
        public async Task<bool> AddressBelongsToUserAsync(int addressId, int userId)
        {
            return await _addressRepository.UserOwnsAddressAsync(addressId, userId);
        }

        protected override async Task<AddressDto> MapToDto(Address address)
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
        protected override async Task<IEnumerable<AddressDto>> MapToDtoList(IEnumerable<Address> entities)
        {
            var addressDtos = new List<AddressDto>();
            foreach (var address in entities)
            {
                addressDtos.Add(await MapToDto(address));
            }
            return addressDtos;
        }
        protected override Address MapToEntity(CreateAddressDto createDto)
        {
            return new Address
            {
                HouseNumber = createDto.HouseNumber,
                StreetBlock = createDto.StreetBlock,
                Area = createDto.Area,
                City = createDto.City,
                Province = createDto.Province,
                Country = createDto.Country,
                ZipCode = createDto.ZipCode,
                UserId = createDto.UserId
            };
        }
        protected override void UpdateEntity(Address entity, UpdateAddressDto updateDto)
        {
            entity.HouseNumber = updateDto.HouseNumber;
            entity.StreetBlock = updateDto.StreetBlock;
            entity.Area = updateDto.Area;
            entity.City = updateDto.City;
            entity.Province = updateDto.Province;
            entity.Country = updateDto.Country;
            entity.ZipCode = updateDto.ZipCode;
        }
        protected override async Task ValidateBeforeCreateAsync(CreateAddressDto createDto)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == createDto.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {createDto.UserId} does not exist");
        }
    }
}
