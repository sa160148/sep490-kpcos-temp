namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionTask
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;

    public Guid ConstructionItemId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Reason { get; set; }

    public Guid? StaffId { get; set; }

    public DateTime? DeadlineAt { get; set; }

    public DateTime? DeadlineActualAt { get; set; }

    public string? Status { get; set; }

    public virtual ConstructionItem ConstructionItem { get; set; } = null!;

    public virtual Staff? Staff { get; set; }
}
