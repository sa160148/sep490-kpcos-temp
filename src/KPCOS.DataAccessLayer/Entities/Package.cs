using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer;

public partial class Package
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public int? Rate { get; set; }

    public string? Description { get; set; }

    public int? Price { get; set; }

    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
}
