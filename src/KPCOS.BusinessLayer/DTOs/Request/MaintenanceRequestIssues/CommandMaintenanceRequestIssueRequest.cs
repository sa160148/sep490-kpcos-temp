namespace KPCOS.BusinessLayer.DTOs.Request.MaintenanceRequestIssues;

public class CommandMaintenanceRequestIssueRequest
{
    public Guid? Id { get; set; }
    public Guid? MaintenanceRequestId { get; set; }
    public Guid? StaffId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Solution { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public bool? IsActive { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public DateOnly? ActualAt { get; set; } 
}