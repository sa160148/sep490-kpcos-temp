using KPCOS.BusinessLayer.DTOs.Request.DocsType;
using KPCOS.BusinessLayer.DTOs.Response.DocsType;
using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DocsTypeController : ControllerBase
{
    private readonly IDocTypeService _docTypeService;
    
    public DocsTypeController(IDocTypeService docTypeService)
    {
        _docTypeService = docTypeService;
    }
    
    [HttpGet("")]
    public async Task<PagedApiResponse<DocsTypeResponse>> GetDocsTypesAsync([FromQuery] GetAllDocsTypeFilterRequest filter)
    {
        var result = await _docTypeService.GetsAsyncPaging(filter);
        return new PagedApiResponse<DocsTypeResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }
    
    [HttpPost("")]
    public async Task<ApiResult> CreateDocsTypeAsync(DocsTypeRequest request)
    {
        await _docTypeService.CreateDocTypeAsync(request);
        return Ok();
    }
    
}