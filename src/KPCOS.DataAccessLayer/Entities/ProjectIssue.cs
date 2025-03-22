using System;

namespace KPCOS.DataAccessLayer.Entities;

public partial class ProjectIssue
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Solution { get; set; }
    public string? Reason { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public string? Status { get; set; }
    public Guid IssueTypeId { get; set; }
    public Guid ConstructionItemId { get; set; }
    public Guid? StaffId { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public DateOnly? ActualAt { get; set; }
    public virtual IssueType IssueType { get; set; } = null!;
    public virtual ConstructionItem ConstructionItem { get; set; } = null!;
    /// <summary>
    /// This is a staff who was assigned to solve the issue
    /// </summary>
    public virtual Staff? Staff { get; set; }
}
