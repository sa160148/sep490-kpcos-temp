using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer;

public partial class Customer
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsActive { get; set; }

    public int? Point { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
