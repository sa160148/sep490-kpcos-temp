namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenanceItem
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<MaintenancePackageItem> MaintenancePackageItems { get; set; } = new List<MaintenancePackageItem>();
    public virtual ICollection<MaintenanceRequestTask> MaintenanceRequestTasks { get; set; } = new List<MaintenanceRequestTask>();
}
