using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenanceItem
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<MaintenancePackageItem> MaintenancePackageItems { get; set; } = new List<MaintenancePackageItem>();
}
