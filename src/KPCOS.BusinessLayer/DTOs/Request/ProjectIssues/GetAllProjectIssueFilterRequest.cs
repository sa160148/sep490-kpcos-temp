using System;
using System.Linq.Expressions;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

public class GetAllProjectIssueFilterRequest : PaginationRequest<ProjectIssue>
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? IssueTypeId { get; set; }
    public Guid? ConstructionItemId { get; set; }
    public Guid? UserId { get; set; }
    
    public override Expression<Func<ProjectIssue, bool>> GetExpressions()
    {
        return issue => 
            (string.IsNullOrEmpty(Search) || 
                (issue.Name != null && issue.Name.Contains(Search)) || 
                (issue.Description != null && issue.Description.Contains(Search)) || 
                (issue.Solution != null && issue.Solution.Contains(Search)) ||
                (issue.Reason != null && issue.Reason.Contains(Search))) &&
            (string.IsNullOrEmpty(Status) || issue.Status == Status) &&
            (!IssueTypeId.HasValue || issue.IssueTypeId == IssueTypeId.Value) &&
            (!ConstructionItemId.HasValue || issue.ConstructionItemId == ConstructionItemId.Value) &&
            (!UserId.HasValue || issue.UserId == UserId.Value) &&
            (issue.IsActive == true);
    }
}