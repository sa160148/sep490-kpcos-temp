using System;

namespace KPCOS.DataAccessLayer.Entities;

public partial class IssueImage
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid ProjectIssueId { get; set; }
    public virtual ProjectIssue ProjectIssue { get; set; }
}
