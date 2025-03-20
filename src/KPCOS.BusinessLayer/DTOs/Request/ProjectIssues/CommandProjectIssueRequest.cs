using System;

namespace KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

public class CommandProjectIssueRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? Solution { get; set; }
    public string? Status { get; set; }
    public bool? IsSolved { get; set; }
    public Guid? IssueTypeId { get; set; }
    public List<CommandIssueImage>? IssueImages { get; set; }
}
