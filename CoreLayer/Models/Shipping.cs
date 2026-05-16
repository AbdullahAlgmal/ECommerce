using System;
using System.Collections.Generic;

namespace ECommerceApi;

public partial class Shipping
{
    public int Id { get; set; }

    public DateOnly ShippingDate { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public byte Status { get; set; }

    public string CarrierName { get; set; } = null!;

    public string TrackingNumber { get; set; } = null!;

    public int AddressId { get; set; }

    public int OrderId { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
