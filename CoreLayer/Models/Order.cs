using System;
using System.Collections.Generic;

namespace DataAccessLayer;

public partial class Order
{
    public int Id { get; set; }

    public DateOnly OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public byte Status { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Shipping> Shippings { get; set; } = new List<Shipping>();

    public virtual User User { get; set; } = null!;
}
