using System.Security.Claims;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Request.Quotations;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
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

    // [CustomAuthorize("CONSULTANT")]
    [HttpPost("")]
    public async Task<ApiResult> CreateQuotationAsync(QuotationCreateRequest request)
    {
        
        
        await _quotationService.CreateQuotationAsync(request);
        return Ok();
       
    }
        
        
    // [CustomAuthorize("ADMINISTRATOR")]
    [HttpGet("")]
    public async Task<PagedApiResponse<QuotationResponse>> GetsAsyncPaging([FromQuery] GetAllQuotationFilterRequest filter)
    {
        var result = await _quotationService.GetsAsyncPaging(filter);
        return new PagedApiResponse<QuotationResponse>(result.Data, filter.page, filter.per_page, result.TotalRecords);
    }
        
    [HttpGet("{id}")]
    public async Task<ApiResult<QuotationResponse>> GetQuotationByIdAsync(Guid id)
    {
        var result = await _quotationService.GetQuotationByIdAsync(id);
        return result;
    }
    
    
    // [CustomAuthorize("ADMINISTRATOR")]
    [HttpPut("{id}/reject-accept")]
    public async Task<ApiResult> RejectOrAcceptQuotationAsync(Guid id, QuotationRejectOrAcceptRequest request)
    {
        await _quotationService.RejectOrAcceptQuotationAsync(id, request);
        return Ok();
    }
    
    
    
    // [CustomAuthorize("CUSTOMER")]
    [HttpPut("{id}/approve-edit")]
    public async Task<ApiResult> ApproveOrCancelEditQuotationAsync(Guid id, QuotationApproveOrEditRequest request)
    {
        await _quotationService.ApproveOrCancelEditQuotationAsync(id, request);
        return Ok();
    }
    
    [HttpPut("{id}/edit")]
    public async Task<ApiResult> UpdateQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        await _quotationService.UpdateQuotationAsync(id, request);
        return Ok();
    }
    
    [HttpPut("{id}/rewrite")]
    public async Task<ApiResult> RewriteQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        await _quotationService.RewriteQuotationAsync(id, request);
        return Ok();
    }

        
       
}