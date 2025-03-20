using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/project-issues")]
    public class ProjectIssuesController : BaseController
    {
        private readonly IProjectIssueService _projectIssueService;

        public ProjectIssuesController(IProjectIssueService projectIssueService)
        {
            _projectIssueService = projectIssueService;
        }
        
        [HttpPost("{id}")]
        public async Task<ApiResult> CreateProjectIssueAsync(
            Guid id, 
            [FromBody] CommandProjectIssueRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("User ID is not valid");
            }
            await _projectIssueService.CreateProjectIssueAsync(id, request, userId);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ApiResult> UpdateProjectIssueAsync(
            Guid id,
            [FromBody] CommandProjectIssueRequest request)
        {
            await _projectIssueService.UpdateProjectIssueAsync(id, request);
            return Ok();
        }

        [HttpDelete("images/{id}")]
        public async Task<ApiResult> DeleteIssueImageAsync(Guid id)
        {
            await _projectIssueService.DeleteIssueImageAsync(id);
            return Ok();
        }
    }
}
