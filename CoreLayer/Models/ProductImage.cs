using System;
using System.Collections.Generic;

namespace ECommerceApi;

public partial class ProductImage
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public byte ImageOrder { get; set; }

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;
}
