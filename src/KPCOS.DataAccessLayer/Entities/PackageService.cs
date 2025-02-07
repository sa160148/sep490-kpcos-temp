using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer;

public partial class PackageService
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsActive { get; set; }

    public int? Price { get; set; }

    public int? Category { get; set; }

    public int? Quantity { get; set; }

    public int? Amount { get; set; }

    public Guid PackageId { get; set; }

    public Guid ServiceId { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
