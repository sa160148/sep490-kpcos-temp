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
        return new PagedApiResponse<QuotationResponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
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
    
    /// <summary>
    /// Updates an existing quotation with new details
    /// </summary>
    /// <param name="id">The unique identifier of the quotation to update</param>
    /// <param name="request">The quotation details for update
    /// <example>
    /// 
    ///     {
    ///         "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "templateConstructionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "services": [
    ///              {
    ///                 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///                 "note": "string",
    ///                 "quantity": 0,
    ///                 "category": "string"
    ///             }
    ///         ],
    ///         "equipments": [
    ///             {
    ///                 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///                 "note": "string",
    ///                 "quantity": 0,
    ///                 "price": 0,
    ///                 "category": "string"
    ///             }
    ///         ]
    ///     } 
    /// </example>
    /// </param>
    /// <returns>Returns OK if the update is successful</returns>
    /// <remarks>
    /// This api accept design with status PREVIEWING
    ///     
    /// </remarks>
    [HttpPut("{id}/edit")]
    public async Task<ApiResult> UpdateQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        await _quotationService.UpdateQuotationAsync(id, request);
        return Ok();
    }
    
    /// <summary>
    /// Rewrites an existing quotation with completely new details, creating a new version
    /// </summary>
    /// <param name="id">The unique identifier of the quotation to rewrite</param>
    /// <param name="request">The new quotation details
    /// <example>
    /// 
    /// {
    ///   "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "templateConstructionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "services": [
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "note": "string",
    ///       "quantity": 0,
    ///       "category": "string"
    ///     }
    ///   ],
    ///   "equipments": [
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "note": "string",
    ///       "quantity": 0,
    ///       "price": 0,
    ///       "category": "string"
    ///     }
    ///   ]
    /// }
    /// </example>
    /// </param>
    /// <returns>Returns OK if the rewrite is successful</returns>
    /// <remarks>
    /// This api accept design with status REJECTED
    ///     
    /// </remarks>
    [HttpPut("{id}/rewrite")]
    public async Task<ApiResult> RewriteQuotationAsync(Guid id, QuotationCreateRequest request)
    {
        await _quotationService.RewriteQuotationAsync(id, request);
        return Ok();
    }

        
       
}