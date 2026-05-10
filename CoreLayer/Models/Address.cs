using System;
using System.Collections.Generic;

namespace DataAccessLayer;

public partial class Address
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

    public virtual ICollection<Shipping> Shippings { get; set; } = new List<Shipping>();

    public virtual User User { get; set; } = null!;
}
