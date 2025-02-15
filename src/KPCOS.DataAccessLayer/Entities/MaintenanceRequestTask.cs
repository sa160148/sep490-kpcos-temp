namespace KPCOS.DataAccessLayer.Entities;

public partial class MaintenanceRequestTask
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid MaintenanceRequestId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid StaffId { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
