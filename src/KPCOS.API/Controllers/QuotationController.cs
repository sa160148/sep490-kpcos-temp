using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class QuotationController : BaseController
{
    private readonly IQuotationService _quotationService;

    public QuotationController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    [CustomAuthorize("CONSULTANT")]
    [HttpPost("")]
    public async Task<ApiResult> CreateQuotationAsync(QuotationCreateRequest request)
    {
        await _quotationService.CreateQuotationAsync(request);
        return Ok();
       
    }
        
        
    // [CustomAuthorize("ADMINISTRATOR")]
    [HttpGet("")]
    public async Task<PagedApiResponse<QuotationResponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _quotationService.GetsAsyncPaging(filter);
        return new PagedApiResponse<QuotationResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
    }
        
    [HttpGet("{id}")]
    public async Task<ApiResult<QuotationResponse>> GetQuotationByIdAsync(Guid id)
    {
        var result = await _quotationService.GetQuotationByIdAsync(id);
        return result;
    }

        
       
}