using System;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;

public class GetAllProjectIssueResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? Solution { get; set; }
    public string? Status { get; set; }
    public GetIssueTypeResponse? IssueType { get; set; }
    public GetConstructionItemForTaskResponse? ConstructionItem { get; set; }
    public IEnumerable<GetAllIssueImageResponse>? IssueImages { get; set; } = new List<GetAllIssueImageResponse>();
    public GetAllStaffResponse? User { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
