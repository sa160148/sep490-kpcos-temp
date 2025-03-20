using System;

namespace KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;

public class GetIssueTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
