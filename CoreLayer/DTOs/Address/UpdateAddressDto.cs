namespace CoreLayer.DTOs.Address
{
    public class UpdateAddressDto
    {
        public string HouseNumber { get; set; } = null!;
        public string StreetBlock { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
    }
}
