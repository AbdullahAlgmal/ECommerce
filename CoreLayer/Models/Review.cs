using System;
using System.Collections.Generic;

namespace DataAccessLayer;

public partial class Review
{
    public int Id { get; set; }

    public string ReviewText { get; set; } = null!;

    public decimal Rating { get; set; }

    public DateOnly ReviewDate { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
