using System;

namespace KPCOS.DataAccessLayer.Entities;

public class MaintenanceRequestIssue
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Solution { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public DateOnly? ActualAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsActive { get; set; }
    public Guid? StaffId { get; set; }
    public Guid MaintenanceRequestId { get; set; }
    public virtual Staff? Staff { get; set; }
    public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;
}