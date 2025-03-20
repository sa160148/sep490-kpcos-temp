using System;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

namespace KPCOS.BusinessLayer.Services;

public interface IProjectIssueService
{
    Task CreateProjectIssueAsync(Guid constructionItemId, CommandProjectIssueRequest request, Guid userId);
    Task UpdateProjectIssueAsync(Guid id, CommandProjectIssueRequest request);
    Task DeleteIssueImageAsync(Guid id);
}
