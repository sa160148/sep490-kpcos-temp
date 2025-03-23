using System;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.DTOs.Response.Users;

namespace KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;

public class GetAllProjectIssueResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Cause { get; set; }
    public string? Reason { get; set; }
    public string? Solution { get; set; }
    public string? Status { get; set; }
    public string? IssueImage { get; set; }
    public string? ConfirmImage { get; set; }
    public string? IssueType { get; set; }
    /*
    public GetIssueTypeResponse? IssueType { get; set; }
    */
    public GetConstructionItemForTaskResponse? ConstructionItem { get; set; }
    public GetAllStaffResponse? Staff { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
