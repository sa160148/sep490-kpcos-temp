using System;

namespace KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

public class CommandProjectIssueRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Reason { get; set; }
    public string? Solution { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public DateOnly? EstimateAt { get; set; }
    public DateOnly? ActualAt { get; set; }
    public Guid? IssueTypeId { get; set; }
    public Guid? StaffId { get; set; }
}
