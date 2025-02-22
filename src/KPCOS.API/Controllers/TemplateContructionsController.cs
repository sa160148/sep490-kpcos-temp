using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;



[ApiController]
[Route("api/[controller]")]
public class TemplateContructionsController : BaseController
{
    private readonly ITemplateContructionService _templateContructionService;
    public TemplateContructionsController(ITemplateContructionService templateContructionService)
    {
        _templateContructionService = templateContructionService;
    }
    
    [HttpPost("")]
    public async Task<ApiResult> CreateTemplateContructionAsync(TemplateContructionCreateRequest request)
    {
        await _templateContructionService.CreateTemplateContructionAsync(request);
        return Ok();
    }
    
    [HttpPost("items")]
    public async Task<ApiResult> CreateTemplateContructionItemAsync(TemplateContructionItemCreateRequest request)
    {
        await _templateContructionService.CreateTemplateContructionItemAsync(request);
        return Ok();
    }
    
    [HttpGet("")]
    public async Task<PagedApiResponse<TemplateContructionResponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        var result = await _templateContructionService.GetsAsyncPaging(filter);
        return new PagedApiResponse<TemplateContructionResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }
    
    [HttpGet("{id}")]
    public async Task<ApiResult<TemplateContructionDetailResponse>> GetTemplateContructionByIdAsync(Guid id)
    {
        var templateContruction = await _templateContructionService.GetTemplateContructionByIdAsync(id);
        return templateContruction;
    }
    
    
}