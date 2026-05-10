namespace CoreLayer.DTOs.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; } = null!;
        public string StreetBlock { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
    }
}
