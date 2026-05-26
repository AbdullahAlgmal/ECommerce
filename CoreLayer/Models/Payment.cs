using System;
using System.Collections.Generic;

namespace DataAccessLayer;

public partial class Payment
{
    public int Id { get; set; }

    public DateOnly PaymentDate { get; set; }

    public string Method { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
