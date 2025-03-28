namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenanceRequest
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Name { get; set; }
    public double Area { get; set; }
    public double Depth { get; set; }
    public string? Address { get; set; }
    public int TotalValue { get; set; }
    public string? Type { get; set; }
    public bool? IsPaid { get; set; }
    public Guid CustomerId { get; set; }

    public Guid MaintenancePackageId { get; set; }

    public string? Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual MaintenancePackage MaintenancePackage { get; set; } = null!;

    public virtual ICollection<MaintenanceRequestTask> MaintenanceRequestTasks { get; set; } = new List<MaintenanceRequestTask>();
}
