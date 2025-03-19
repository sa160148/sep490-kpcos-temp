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

    public string? Solution { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public Guid IssueTypeId { get; set; }
    public Guid ConstructionItemId { get; set; }
    public Guid UserId { get; set; }
    public virtual IssueType IssueType { get; set; } = null!;
    public virtual ConstructionItem ConstructionItem { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
