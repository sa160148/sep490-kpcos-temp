namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenancePackageItem
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid MaintenancePackageId { get; set; }

    public Guid MaintenanceItemId { get; set; }

    public virtual MaintenanceItem MaintenanceItem { get; set; } = null!;

    public virtual MaintenancePackage MaintenancePackage { get; set; } = null!;
}
