using KPCOS.BusinessLayer.DTOs.Request.IssueTypes;
using KPCOS.BusinessLayer.DTOs.Response.IssueTypes;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;




[ApiController]
[Route("api/[controller]")]
public class IssueTypeController : ControllerBase
{
    private readonly IIssueTypeService _issueTypeService;

    public IssueTypeController(IIssueTypeService issueTypeService)
    {
        _issueTypeService = issueTypeService;
    }

    [HttpGet("")]
    public async Task<PagedApiResponse<IssueTypeResponse>> GetIssueTypesAsync([FromQuery] GetAllIssueTypeFilterRequest filter)
    {
        var result = await _issueTypeService.GetsAsyncPaging(filter);
        return new PagedApiResponse<IssueTypeResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }

    [HttpPost("")]
    public async Task<ApiResult> CreateIssueTypeAsync(IssueTypeRequest request)
    {
        await _issueTypeService.CreateIssueTypeAsync(request);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> UpdateIssueTypeAsync(Guid id, IssueTypeRequest request)
    {
        await _issueTypeService.UpdateIssueTypeAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> DeleteIssueTypeAsync(Guid id)
    {
        await _issueTypeService.DeleteIssueTypeAsync(id);
        return Ok();
    }
    
}