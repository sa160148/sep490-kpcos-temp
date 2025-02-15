namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenancePackage : BaseEntity
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int PricePerUnit { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<MaintenancePackageItem> MaintenancePackageItems { get; set; } = new List<MaintenancePackageItem>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
}
