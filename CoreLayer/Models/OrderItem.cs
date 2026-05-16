using System;
using System.Collections.Generic;

namespace ECommerceApi;

public partial class OrderItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
