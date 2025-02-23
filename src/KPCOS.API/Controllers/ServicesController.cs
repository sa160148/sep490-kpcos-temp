using KPCOS.BusinessLayer;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.Services;
using KPCOS.Common;
using KPCOS.Common.Pagination;
using KPCOS.WebFramework.Api;

using KPCOS.WebFramework.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ServicesController : BaseController
{
    private readonly IServiceService _serviceService;
    
    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }
    [CustomAuthorize("ADMINISTRATOR")] 
    [HttpPost("")]
    public async Task<ApiResult> CreateServiceAsync(ServiceCreateRequest request)
    {
        await _serviceService.CreateService(request);
        return new ApiResult(true, ApiResultStatusCode.Success);
    }
    
    // [HttpGet("")]
    // public async Task<ApiResult<PaginationResult<ServiceReponse>>> GetsAsync([FromQuery] PaginationFilter filter)
    // {
    //     
    //     var result = await _serviceService.GetsAsync(filter);
    //     return result;
    // }  
    
    [HttpGet("")]
   
    public async Task<PagedApiResponse<ServiceReponse>> GetsAsyncPaging([FromQuery] PaginationFilter filter)
    {
        
        var result = await _serviceService.GetsAsyncPaging(filter);
        return new PagedApiResponse<ServiceReponse>(result.Data, filter.PageNumber, filter.PageSize, result.TotalRecords);
        
    }   


    [HttpGet("{id}")]
    public async Task<ApiResult<ServiceReponse>> GetServiceByIdAsync(Guid id)
    {
        var result = await _serviceService.GetServiceByIdAsync(id);
        return result;
    }
    

    [HttpPut("{id}")]
    public async Task<ApiResult> UpdateServiceAsync(Guid id, ServiceCreateRequest request)
    {
        await _serviceService.UpdateServiceAsync(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> DeleteServiceAsync(Guid id)
    {
        await _serviceService.DeleteServiceAsync(id);
        return Ok();
    }
}