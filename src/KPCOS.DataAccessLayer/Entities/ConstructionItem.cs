namespace KPCOS.DataAccessLayer.Entities;

public partial class ConstructionItem
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Name { get; set; } = null!;
    public string? Category { get; set; }

    public string? Description { get; set; }

    public DateOnly EstimateAt { get; set; }

    public DateOnly? ActualAt { get; set; }

    public Guid? ParentId { get; set; }

    public Guid ProjectId { get; set; }

    public bool? IsPayment { get; set; }
    
    public string? Status { get; set; }

    public virtual ICollection<ConstructionTask> ConstructionTasks { get; set; } = new List<ConstructionTask>();
    public virtual ICollection<ProjectIssue> ProjectIssues { get; set; } = new List<ProjectIssue>();

    public virtual Project Project { get; set; } = null!;
}
